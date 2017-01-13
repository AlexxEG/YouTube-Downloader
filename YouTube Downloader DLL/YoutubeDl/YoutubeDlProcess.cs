using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using YouTube_Downloader_DLL.Classes;

namespace YouTube_Downloader_DLL.YoutubeDl
{
    public class YoutubeDlProcess
    {
        public static class Commands
        {
            public const string Download = " -o \"{0}\" --hls-prefer-native -f {1} {2}";
            public const string GetJsonInfo = " -o \"{0}\\%(title)s\" --no-playlist --skip-download --restrict-filenames --write-info-json \"{1}\"{2}";
            public const string Authentication = " -u {0} -p {1}";
            public const string TwoFactor = " -2 {0}";
            public const string Update = " -U";
            public const string Version = " --version";
        }

        public static string YouTubeDlPath = Path.Combine(Application.StartupPath, "Externals", "youtube-dl.exe");

        OperationLogger _logger;
        YTDAuthentication _authentication;

        public YoutubeDlProcess(OperationLogger logger, YTDAuthentication authentication)
        {
            _logger = logger;
            _authentication = authentication;
        }

        /// <summary>
        /// Writes log footer to log.
        /// </summary>
        public void LogFooter()
        {
            // Write log footer to stream.
            // Possibly write elapsed time and/or error in future.
            _logger?.LogLine("-" + Environment.NewLine);
        }

        /// <summary>
        /// Writes log header to log.
        /// </summary>
        /// <param name="arguments">The arguments to log in header.</param>
        /// <param name="url">The URL to log in header.</param>
        public void LogHeader(string arguments, [CallerMemberName]string caller = "")
        {
            _logger?.LogLine("[" + DateTime.Now + "]");
            _logger?.LogLine("version: " + GetVersion());
            _logger?.LogLine("caller: " + caller);
            _logger?.LogLine("cmd: " + arguments.Trim());
            _logger?.LogLine();
            _logger?.LogLine("OUTPUT");
        }

        /// <summary>
        /// Returns a <see cref="VideoInfo"/> of the given video.
        /// </summary>
        /// <param name="url">The url to the video.</param>
        public VideoInfo GetVideoInfo(string url)
        {
            string json_dir = Common.GetJsonDirectory();
            string json_file = string.Empty;
            string arguments = string.Format(Commands.GetJsonInfo,
                json_dir,
                url,
                _authentication == null ? string.Empty : _authentication.ToCmdArgument());
            VideoInfo video = new VideoInfo();

            LogHeader(arguments);

            Helper.StartProcess(YouTubeDlPath, arguments,
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
                }, null, _logger)
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

            Helper.StartProcess(YouTubeDlPath, Commands.Version,
                delegate (Process process, string line)
                {
                    // Only one line gets printed, so assume any non-empty line is the version
                    if (!string.IsNullOrEmpty(line))
                        version = line.Trim();
                },
                null, null, null).WaitForExit();

            return version;
        }

        public static async Task<string> Update()
        {
            string returnMsg = string.Empty;
            Regex versionRegex = new Regex(@"(\d{4}\.\d{2}\.\d{2})");
            OperationLogger logger = OperationLogger.Create(OperationLogger.YTDLogFile);

            await Task.Run(delegate
            {
                Helper.StartProcess(YouTubeDlPath, Commands.Update,
                    delegate (Process process, string line)
                    {
                        Match m;
                        if ((m = versionRegex.Match(line)).Success)
                            returnMsg = m.Groups[1].Value;
                    },
                    delegate (Process process, string line)
                    {
                        returnMsg = "Failed";
                    }, null, logger)
                    .WaitForExit();
            });

            return returnMsg;
        }
    }
}
