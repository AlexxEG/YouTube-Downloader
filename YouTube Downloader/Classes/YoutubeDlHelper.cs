using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace YouTube_Downloader.Classes
{
    public class YoutubeDlHelper
    {
        private const string Cmd_JSON_Info = " -o \"{0}\\%(title)s\" --no-playlist --skip-download --restrict-filenames --write-info-json \"{1}\"";
        private const string Log_Filename = "youtube-dl.log";

        private static string YouTubeDlPath = Path.Combine(Application.StartupPath, "externals", "youtube-dl.exe");

        private static FileStream _logWriter;

        /// <summary>
        /// Returns the <see cref="FileStream"/> for the youtube-dl log file, initializing it if necessary.
        /// </summary>
        public static FileStream GetLogWriter()
        {
            if (_logWriter != null)
                return _logWriter;

            string filename = Path.Combine(Program.GetLogsDirectory(), Log_Filename);

            _logWriter = new FileStream(filename, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);

            return _logWriter;
        }

        /// <summary>
        /// Returns a <see cref="VideoInfo"/> of the given video.
        /// </summary>
        /// <param name="url">The url to the video.</param>
        public static VideoInfo GetVideoInfo(string url)
        {
            string json_dir = Program.GetJsonDirectory();

            /* Fill in json directory & video url. */
            string arguments = string.Format(Cmd_JSON_Info, json_dir, url);

            Process process = StartProcess(arguments);

            string json_file = "";
            string line = "";
            var sb = new StringBuilder();

            while ((line = process.StandardOutput.ReadLine()) != null)
            {
                sb.AppendLine(line);

                line = line.Trim();

                if (line.StartsWith("[info] Writing video description metadata as JSON to:"))
                {
                    /* Store file path. */
                    json_file = line.Substring(line.IndexOf(":") + 1).Trim();
                }
            }

            /* Write output to log. */
            lock (GetLogWriter())
            {
                WriteLogHeader(arguments, url);
                WriteLogText(sb.ToString());
                WriteLogFooter();
            }

            process.WaitForExit();

            if (!process.HasExited)
                process.Kill();

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
        /// Creates a Process with the given arguments, then returns it after it has started.
        /// </summary>
        public static Process StartProcess(string arguments)
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

            var process = new Process()
            {
                StartInfo = psi
            };

            return process;
        }

        /// <summary>
        /// Writes log footer to log.
        /// </summary>
        public static void WriteLogFooter()
        {
            // Write log footer to stream.
            // Possibly write elapsed time and/or error in future.
            byte[] bytes = Common.LogEncoding.GetBytes(Environment.NewLine);

            for (int i = 0; i < 3; i++)
            {
                _logWriter.Write(bytes, 0, bytes.Length);
            }

            _logWriter.Flush();
        }

        /// <summary>
        /// Writes log header to log.
        /// </summary>
        /// <param name="arguments">The arguments to log in header.</param>
        /// <param name="url">The URL to log in header.</param>
        public static void WriteLogHeader(string arguments, string url)
        {
            var sb = new StringBuilder();

            sb.AppendLine("[" + DateTime.Now + "]");
            sb.AppendLine("url: " + url);
            sb.AppendLine("cmd: " + arguments);
            sb.AppendLine();
            sb.AppendLine("OUTPUT");

            // Write log header to stream
            byte[] bytes = Common.LogEncoding.GetBytes(sb.ToString());

            _logWriter.Write(bytes, 0, bytes.Length);
            _logWriter.Flush();
        }

        /// <summary>
        /// Writes text to log writer.
        /// </summary>
        /// <param name="text">The text to write to log.</param>
        public static void WriteLogText(string text)
        {
            byte[] bytes = Common.LogEncoding.GetBytes(text);

            _logWriter.Write(bytes, 0, bytes.Length);
            _logWriter.Flush();
        }
    }
}
