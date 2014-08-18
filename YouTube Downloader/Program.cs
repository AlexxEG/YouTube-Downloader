using DeDauwJeroen;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using YouTube_Downloader.Classes;

namespace YouTube_Downloader
{
    static class Program
    {
        public const string Name = "YouTube Downloader";

        public static bool FFmpegAvailable = true;

        /// <summary>
        /// Store running downloaders that can be stopped automatically when closing application.
        /// </summary>
        public static List<FileDownloader> RunningDownloaders = new List<FileDownloader>();
        /// <summary>
        /// Store running background workers that can be stopped automatically when closing application.
        /// </summary>
        public static List<BackgroundWorker> RunningWorkers = new List<BackgroundWorker>();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Program.FFmpegAvailable = File.Exists(FFmpegHelper.FFmpegPath);

            new App().Run(args);
        }

        static void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        {
            Program.SaveException((Exception)e.ExceptionObject);
        }

        /// <summary>
        /// Returns the local app data directory for this program. Also makes sure the directory exists.
        /// </summary>
        /// <returns></returns>
        public static string GetLocalAppDataFolder()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Program.Name);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path;
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
