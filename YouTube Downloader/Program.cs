using System;
using System.IO;
using System.Windows.Forms;

namespace YouTube_Downloader
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
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
}
