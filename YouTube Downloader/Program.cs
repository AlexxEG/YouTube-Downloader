using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using YouTube_Downloader.Classes;
using YouTube_Downloader.Operations;

namespace YouTube_Downloader
{
    public static class Program
    {
        public const string Name = "YouTube Downloader";

        public static bool FFmpegAvailable = true;

        /// <summary>
        /// Store running operations that can be stopped automatically when closing application.
        /// </summary>
        public static List<Operation> RunningOperations = new List<Operation>();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.SetCompatibleTextRenderingDefault(false);

            Program.FFmpegAvailable = File.Exists(FFmpegHelper.FFmpegPath);

            // Up the connection limit for getting the file sizes of video formats
            System.Net.ServicePointManager.DefaultConnectionLimit = 20;

            new App().Run(args);
        }

        /// <summary>
        /// Automatically saves unhandled Exceptions.
        /// </summary>
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Program.SaveException((Exception)e.ExceptionObject);
        }

        /// <summary>
        /// Returns the local app data directory for this program. Also makes sure the directory exists.
        /// </summary>
        public static string GetAppDataDirectory()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Program.Name);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path;
        }

        /// <summary>
        /// Returns the json directory for this program. Also makes sure the directory exists.
        /// </summary>
        public static string GetJsonDirectory()
        {
            string path = Path.Combine(GetAppDataDirectory(), "json");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path;
        }

        /// <summary>
        /// Returns the logs directory for this program. Also makes sure the directory exists.
        /// </summary>
        public static string GetLogsDirectory()
        {
            string path = Path.Combine(GetAppDataDirectory(), "logs");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path;
        }

        /// <summary>
        /// Saves given Exception's stack trace to a readable file in the local application data folder.
        /// </summary>
        /// <param name="ex">The Exception to save.</param>
        public static void SaveException(Exception ex)
        {
            string directory = Path.Combine(GetAppDataDirectory(), "stack traces");

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string dateFormat = "yyyy_MM_dd-HH_mm_ss";
            string file = string.Format("{0}\\stackTrace.{1}.log", directory, DateTime.Now.ToString(dateFormat));

            File.WriteAllText(file, ex.ToString());
        }
    }
}
