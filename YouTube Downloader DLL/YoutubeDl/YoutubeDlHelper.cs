using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using YouTube_Downloader_DLL.Classes;
using YouTube_Downloader_DLL.YoutubeDl;

namespace YouTube_Downloader_DLL.YoutubeDl
{
    public class YoutubeDlHelper
    {
        public static class Commands
        {
            public const string Download = " -o \"{0}\" --hls-prefer-native -f {1} {2}";
            public const string GetJsonInfo = " -o \"{0}\\%(title)s\" --no-playlist --skip-download --restrict-filenames --write-info-json \"{1}\"{2}";
            public const string Authentication = " -u {0} -p {1}";
            public const string TwoFactor = " -2 {0}";
            public const string Version = " --version";
        }

        private static string YouTubeDlPath = Path.Combine(Application.StartupPath, "Externals", "youtube-dl.exe");

        /// <summary>
        /// Creates a Process with the given arguments, then returns it.
        /// </summary>
        public static Process StartProcess(OperationLogger logger,
                                           string arguments,
                                           string caller,
                                           Action<Process, string> output,
                                           Action<Process, string> error)
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
                EnableRaisingEvents = true,
                StartInfo = psi
            };

            process.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
            {
                if (string.IsNullOrEmpty(e.Data))
                    return;

                logger?.Log(e.Data);
                output?.Invoke(process, e.Data);
            };
            process.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs e)
            {
                if (string.IsNullOrEmpty(e.Data))
                    return;

                logger?.Log(e.Data);
                error?.Invoke(process, e.Data);
            };

            logger?.Log(BuildLogHeader(arguments, caller));

            process.Exited += delegate
            {
                logger?.Log(BuildLogFooter());
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            return process;
        }

        /// <summary>
        /// Returns a <see cref="VideoInfo"/> of the given video.
        /// </summary>
        /// <param name="url">The url to the video.</param>
        public static VideoInfo GetVideoInfo(OperationLogger logger,
                                             string url,
                                             YTDAuthentication authentication = null,
                                             [CallerMemberName]string caller = "")
        {
            string json_dir = Common.GetJsonDirectory();
            string json_file = string.Empty;
            string arguments = string.Format(Commands.GetJsonInfo,
                json_dir,
                url,
                authentication == null ? string.Empty : authentication.ToCmdArgument());
            VideoInfo video = new VideoInfo();

            StartProcess(logger, arguments, caller,
                delegate (Process process, string line)
                {
                    line = line.Trim();

                    if (line.StartsWith("[info] Writing video description metadata as JSON to:"))
                    {
                        // Store file path
                        json_file = line.Substring(line.IndexOf(":") + 1).Trim();
                    }
                    else if (line.Contains("Refetching age-gated info webpage"))
                    {
                        video.RequiresAuthentication = true;
                    }
                },
                delegate (Process process, string error)
                {
                    error = error.Trim();

                    if (error.Contains("YouTube said: Please sign in to view this video."))
                    {
                        video.RequiresAuthentication = true;
                    }
                    else if (error.StartsWith("ERROR:"))
                    {
                        video.Failure = true;
                        video.FailureReason = error.Substring("ERROR: ".Length);
                    }
                })
                .WaitForExit();

            if (!video.Failure && !video.RequiresAuthentication)
                video.DeserializeJson(json_file);

            return video;
        }

        /// <summary>
        /// Gets current youtube-dl version.
        /// </summary>
        public static string GetVersion()
        {
            string version = string.Empty;

            StartProcess(null, Commands.Version, string.Empty,
                delegate (Process process, string line)
                {
                    // Only one line gets printed, so assume any non-empty line is the version
                    if (!string.IsNullOrEmpty(line))
                        version = line.Trim();
                },
                null).WaitForExit();

            return version;
        }

        /// <summary>
        /// Writes log footer to log.
        /// </summary>
        public static string BuildLogFooter()
        {
            // Write log footer to stream.
            // Possibly write elapsed time and/or error in future.
            return "-" + Environment.NewLine;
        }

        /// <summary>
        /// Writes log header to log.
        /// </summary>
        /// <param name="arguments">The arguments to log in header.</param>
        /// <param name="url">The URL to log in header.</param>
        public static string BuildLogHeader(string arguments, string caller)
        {
            var sb = new StringBuilder();

            sb.AppendLine("[" + DateTime.Now + "]");
            sb.AppendLine("version: " + GetVersion());
            sb.AppendLine("caller: " + caller);
            sb.AppendLine("cmd: " + arguments.Trim());
            sb.AppendLine();
            sb.AppendLine("OUTPUT");

            return sb.ToString();
        }
    }
}
