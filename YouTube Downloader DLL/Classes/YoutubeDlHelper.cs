using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace YouTube_Downloader_DLL.Classes
{
    public class YoutubeDlHelper
    {
        public static class Commands
        {
            public const string GetJsonInfo = " -o \"{0}\\%(title)s\" --no-playlist --skip-download --restrict-filenames --write-info-json \"{1}\"";
            public const string Version = " --version";
        }

        private const string LogFilename = "youtube-dl.log";

        private static string YouTubeDlPath = Path.Combine(Application.StartupPath, "Externals", "youtube-dl.exe");

        /// <summary>
        /// Creates a Process with the given arguments, then returns it.
        /// </summary>
        public static ProcessLogger CreateProcess(string arguments, bool noLog = false)
        {
            var psi = new ProcessStartInfo(YoutubeDlHelper.YouTubeDlPath, arguments)
            {
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            ProcessLogger process = null;
            string filename = Path.Combine(Common.GetLogsDirectory(), LogFilename);

            if (noLog)
                process = new ProcessLogger();
            else
                process = new ProcessLogger(filename)
                {
                    Header = BuildLogHeader(arguments),
                    Footer = BuildLogFooter()
                };

            process.StartInfo = psi;

            return process;
        }

        /// <summary>
        /// Returns a <see cref="VideoInfo"/> of the given video.
        /// </summary>
        /// <param name="url">The url to the video.</param>
        public static VideoInfo GetVideoInfo(string url)
        {
            string json_dir = Common.GetJsonDirectory();
            string json_file = "";
            string arguments = string.Format(Commands.GetJsonInfo, json_dir, url);

            var process = CreateProcess(arguments);

            process.NewLineOutput += delegate(string line)
            {
                line = line.Trim();

                if (line.StartsWith("[info] Writing video description metadata as JSON to:"))
                {
                    // Store file path
                    json_file = line.Substring(line.IndexOf(":") + 1).Trim();
                }
                else if (line.StartsWith("ERROR:"))
                {

                }
            };
            process.Start();
            process.WaitForExit();

            return new VideoInfo(json_file);
        }

        /// <summary>
        /// Gets video information, then calls the given result method with the result.
        /// </summary>
        /// <param name="url">The url to the video.</param>
        /// <param name="resultMethod">The method to call when result is ready.</param>
        public static async void GetVideoInfoAsync(string url, Action<VideoInfo> resultMethod)
        {
            VideoInfo videoInfo = null;
            await System.Threading.Tasks.Task.Run(delegate
            {
                videoInfo = YoutubeDlHelper.GetVideoInfo(url);
            });
            resultMethod.Invoke(videoInfo);
        }

        /// <summary>
        /// Gets current youtube-dl version.
        /// </summary>
        public static string GetVersion()
        {
            var process = CreateProcess(Commands.Version, true);
            string version = "";

            process.NewLineOutput += delegate(string line)
            {
                // Only one line gets printed, so assume any non-empty line is the version
                if (!string.IsNullOrEmpty(line))
                    version = line.Trim();
            };

            process.Start();
            process.WaitForExit();

            return version;
        }

        /// <summary>
        /// Writes log footer to log.
        /// </summary>
        public static string BuildLogFooter()
        {
            // Write log footer to stream.
            // Possibly write elapsed time and/or error in future.
            return Environment.NewLine;
        }

        /// <summary>
        /// Writes log header to log.
        /// </summary>
        /// <param name="arguments">The arguments to log in header.</param>
        /// <param name="url">The URL to log in header.</param>
        public static string BuildLogHeader(string arguments)
        {
            var sb = new StringBuilder();

            sb.AppendLine("[" + DateTime.Now + "]");
            sb.AppendLine("version: " + GetVersion());
            sb.AppendLine("cmd: " + arguments.Trim());
            sb.AppendLine();
            sb.AppendLine("OUTPUT");

            return sb.ToString();
        }
    }
}
