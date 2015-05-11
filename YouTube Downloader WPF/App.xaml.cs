using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using YouTube_Downloader.Classes;
using YouTube_Downloader.Operations;

namespace YouTube_Downloader_WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const string Name = "YouTube Downloader";

        public static bool FFmpegAvailable = true;

        /// <summary>
        /// Store running operations that can be stopped automatically when closing application.
        /// </summary>
        public static List<Operation> RunningOperations = new List<Operation>();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            this.DispatcherUnhandledException += App_DispatcherUnhandledException;

            // Check for ffmpeg
            App.FFmpegAvailable = File.Exists(FFmpegHelper.FFmpegPath);

            // Up the connection limit for getting the file sizes of video formats
            System.Net.ServicePointManager.DefaultConnectionLimit = 20;
        }

        /// <summary>
        /// Automatically saves unhandled Exceptions.
        /// </summary>
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            App.SaveException(e.Exception);
        }

        /// <summary>
        /// Returns the local app data directory for this program. Also makes sure the directory exists.
        /// </summary>
        public static string GetAppDataDirectory()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), App.Name);

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
