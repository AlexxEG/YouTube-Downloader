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
        /// Downloads Twitch VOD.
        /// </summary>
        /// <param name="output">Where to save the video file.</param>
        /// <param name="format">The video format to download.</param>
        /// <param name="progressUpdateCallback">Callback for progress reporting. Can be null.</param>
        /// <param name="ct">Cancellation token for canceling download. Can be null.</param>
        public static void DownloadTwitchVOD(OperationLogger logger,
                                             string output,
                                             VideoFormat format,
                                             Action<TwitchOperationProgress> progressUpdateCallback,
                                             CancellationToken ct,
                                             [CallerMemberName]string caller = "")
        {
            if (string.IsNullOrEmpty(output))
                throw new ArgumentNullException("output");

            if (format == null)
                throw new ArgumentNullException("format");

            if (format.VideoInfo.VideoSource != VideoSource.Twitch)
                throw new ArgumentException("This method only supports videos from Twitch.", "format");

            string arguments = string.Format(Commands.Download,
                output,
                format.FormatID,
                format.VideoInfo.Url);

            DateTime nextUpdate = DateTime.Now;

            /* Regex for finding progress. Will match these areas:
             * 1 - decimal: Progress percentage
             * 2 - decimal: Total size
             * 3 - string : Total size suffix (KiB/MiB/GiB)
             * 4 - decimal: Speed
             * 5 - string : Speed suffix (KiB/MiB/GiB)
             * 6 - string : ETA (00:00 format) 
             */
            Regex regexUpdate = new Regex(
                @"^\[download\]\s+(\d+\.\d+)%.*~(\d+\.\d+)([K|M|G]iB).*\s(\d+\.\d+)([K|M]iB)\/s.*(\d{2}:\d{2}).*$",
                RegexOptions.Compiled);

            StartProcess(logger, arguments, caller,
                delegate (Process process, string line)
                {
                    line = line.Trim();

                    if (ct != null && ct.IsCancellationRequested)
                    {
                        process.Kill();
                        return;
                    }

                    if (progressUpdateCallback == null)
                        return;

                    if (line.Contains("100%")) // Only the last line will show 100% as "100%" and not "100.0%"
                    {
                        // Regex for finding final file size
                        Regex regexComplete = new Regex(
                            @"^\[download\]\s+100%.*\s+(\d+\.?\d+)([K|M|G]iB).*$");

                        Match m = regexComplete.Match(line);
                        decimal totalSize = decimal.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
                        string suffix = m.Groups[2].Value;

                        progressUpdateCallback.Invoke(new TwitchOperationProgress(
                                100m, totalSize, suffix, 0m, string.Empty, string.Empty
                            ));
                    }
                    else if (nextUpdate <= DateTime.Now) // Only update every 500 ms
                    {
                        Match m;
                        if ((m = regexUpdate.Match(line)).Success)
                        {
                            decimal percentage = decimal.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
                            decimal totalSize = decimal.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture);
                            string totalSizeSuffix = m.Groups[3].Value;
                            decimal speed = decimal.Parse(m.Groups[4].Value, CultureInfo.InvariantCulture);
                            string speedSuffix = m.Groups[5].Value;
                            string eta = m.Groups[6].Value;

                            // Don't allow 100% here, it's reserved for above
                            progressUpdateCallback.Invoke(new TwitchOperationProgress(
                                    Math.Min(99m, percentage), totalSize, totalSizeSuffix, speed, speedSuffix, eta
                                ));

                            nextUpdate = DateTime.Now.AddMilliseconds(500);
                        }
                    }
                },
                delegate (Process process, string error)
                {
                    error = error.Trim();

                    if (error.StartsWith("ERROR:"))
                    {
                        format.VideoInfo.Failure = true;
                        format.VideoInfo.FailureReason = error.Substring("ERROR: ".Length);
                    }
                }).WaitForExit();
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
