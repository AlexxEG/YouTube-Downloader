using System;
using System.IO;
using System.Text;

namespace YouTube_Downloader_DLL.Classes
{
    public class Common
    {
        public const int ProgressUpdateDelay = 250;

        public const string Name = "YouTube Downloader";
        public const string VersionString = "2.0.0";

        public static Encoding LogEncoding = Encoding.UTF8;
        public static Version Version = new Version(VersionString);

        /// <summary>
        /// Returns the local app data directory for this program. Also makes sure the directory exists.
        /// </summary>
        public static string GetAppDataDirectory()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                Common.Name);

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

        public static string GetStackTracesDirectory()
        {
            string path = Path.Combine(GetAppDataDirectory(), "stack traces");

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
            string directory = GetStackTracesDirectory();

            string dateFormat = "yyyy_MM_dd-HH_mm_ss";
            string file = string.Format("{0}\\stackTrace.{1}.log", directory, DateTime.Now.ToString(dateFormat));

            File.WriteAllText(file, ex.ToString());
        }
    }
}
