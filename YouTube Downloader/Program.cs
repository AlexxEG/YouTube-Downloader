using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.IO;
using System.Windows.Forms;

namespace YouTube_Downloader
{
    static class Program
    {
        public static bool FFmpegAvailable = true;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Program.FFmpegAvailable = File.Exists(Path.Combine(Application.StartupPath, "ffmpeg.exe"));

            new App().Run(args);
        }

        static void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        {
            Program.SaveException((Exception)e.ExceptionObject);
        }

        public static void SaveException(Exception ex)
        {
            string directory = Path.Combine(Application.StartupPath, "StackTraces");

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string file = string.Format("{0}\\StackTraces\\stackTrace.{1}.log",
                Application.StartupPath,
                DateTime.Now.ToString("yyyy_MM_dd-HH_mm_ss"));

            File.WriteAllText(file, ex.ToString());
        }
    }

    public class App : WindowsFormsApplicationBase
    {
        public App()
        {
            this.IsSingleInstance = true;
            this.EnableVisualStyles = true;
            this.ShutdownStyle = ShutdownMode.AfterMainFormCloses;
        }

        protected override void OnCreateMainForm()
        {
            string[] args = new string[this.CommandLineArgs.Count];

            this.CommandLineArgs.CopyTo(args, 0);

            if (this.CommandLineArgs.Count > 0)
                this.MainForm = new MainForm(args);
            else
                this.MainForm = new MainForm();
        }

        protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
        {
            eventArgs.BringToForeground = true;
            base.OnStartupNextInstance(eventArgs);
            if (eventArgs.CommandLine.Count > 0)
            {
                MainForm mainForm = (MainForm)this.MainForm;
                mainForm.InsertVideo(eventArgs.CommandLine[0]);
            }
        }
    }
}
