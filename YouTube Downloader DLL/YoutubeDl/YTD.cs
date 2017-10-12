using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using YouTube_Downloader_DLL.Classes;
using System.Collections.Specialized;
using System.Collections;

namespace YouTube_Downloader_DLL.YoutubeDl
{
    public class YTD
    {
        public static class Commands
        {
            public const string Download = " -o \"{0}\" --hls-prefer-native -f {1} {2}";
            public const string GetJsonInfo = " -o \"{0}\\%(title)s\" --no-playlist --skip-download --restrict-filenames --write-info-json \"{1}\"{2}";
            public const string GetJsonInfoBatch = " -o \"{0}\\%(id)s_%(title)s\" --no-playlist --skip-download --restrict-filenames --write-info-json {1}";
            public const string Authentication = " -u {0} -p {1}";
            public const string TwoFactor = " -2 {0}";
            public const string Update = " -U";
            public const string Version = " --version";
        }

        private const string ErrorSignIn = "YouTube said: Please sign in to view this video.";

        public static string YouTubeDlPath = Path.Combine(Application.StartupPath, "Externals", "youtube-dl.exe");

        /// <summary>
        /// Writes log footer to log.
        /// </summary>
        public static void LogFooter(OperationLogger logger)
        {
            // Write log footer to stream.
            // Possibly write elapsed time and/or error in future.
            logger?.LogLine("-" + Environment.NewLine);
        }

        /// <summary>
        /// Writes log header to log.
        /// </summary>
        /// <param name="arguments">The arguments to log in header.</param>
        /// <param name="url">The URL to log in header.</param>
        public static void LogHeader(OperationLogger logger, string arguments, [CallerMemberName]string caller = "")
        {
            logger?.LogLine("[" + DateTime.Now + "]");
            logger?.LogLine("version: " + GetVersion());
            logger?.LogLine("caller: " + caller);
            logger?.LogLine("cmd: " + arguments.Trim());
            logger?.LogLine();
            logger?.LogLine("OUTPUT");
        }

        /// <summary>
        /// Returns a <see cref="VideoInfo"/> of the given video.
        /// </summary>
        /// <param name="url">The url to the video.</param>
        public static VideoInfo GetVideoInfo(string url,
                                             YTDAuthentication authentication = null,
                                             OperationLogger logger = null)
        {
            string json_dir = Common.GetJsonDirectory();
            string json_file = string.Empty;
            string arguments = string.Format(Commands.GetJsonInfo,
                json_dir,
                url,
                authentication == null ? string.Empty : authentication.ToCmdArgument());
            VideoInfo video = new VideoInfo();

            LogHeader(logger, arguments);

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

                    if (error.Contains(ErrorSignIn))
                    {
                        video.RequiresAuthentication = true;
                    }
                    else if (error.StartsWith("ERROR:"))
                    {
                        video.Failure = true;
                        video.FailureReason = error.Substring("ERROR: ".Length);
                    }
                }, null, logger)
                .WaitForExit();

            if (!video.Failure && !video.RequiresAuthentication)
                video.DeserializeJson(json_file);

            return video;
        }

        public static async Task GetVideoInfoBatchAsync(ICollection<string> urls,
                                                        Action<VideoInfo> videoReady,
                                                        YTDAuthentication authentication = null,
                                                        OperationLogger logger = null)
        {
            string json_dir = Common.GetJsonDirectory();
            string arguments = string.Format(Commands.GetJsonInfoBatch, json_dir, string.Join(" ", urls));
            var videos = new OrderedDictionary();
            var jsonFiles = new Dictionary<string, string>();
            var findVideoID = new Regex(@"(?:\]|ERROR:)\s(.{11}):", RegexOptions.Compiled);
            var findVideoIDJson = new Regex(@":\s.*\\(.*?)_", RegexOptions.Compiled);

            LogHeader(logger, arguments);

            await Task.Run(() =>
            {
                Helper.StartProcess(YouTubeDlPath, arguments,
                        (Process process, string line) =>
                        {
                            line = line.Trim();
                            Match m;
                            string id;
                            VideoInfo video = null;

                            if ((m = findVideoID.Match(line)).Success)
                            {
                                id = findVideoID.Match(line).Groups[1].Value;
                                video = videos.Get<VideoInfo>(id, new VideoInfo() { ID = id });
                            }

                            if (line.StartsWith("[info] Writing video description metadata as JSON to:"))
                            {
                                id = findVideoIDJson.Match(line).Groups[1].Value;
                                var jsonFile = line.Substring(line.IndexOf(":") + 1).Trim();
                                jsonFiles.Put(id, jsonFile);

                                video = videos[id] as VideoInfo;
                                video.DeserializeJson(jsonFile);
                                videoReady(video);
                            }
                            else if (line.Contains("Refetching age-gated info webpage"))
                            {
                                video.RequiresAuthentication = true;
                            }
                        },
                        (Process process, string error) =>
                        {
                            error = error.Trim();
                            var id = findVideoID.Match(error).Groups[1].Value;
                            var video = videos.Get<VideoInfo>(id, new VideoInfo() { ID = id });

                            if (error.Contains(ErrorSignIn))
                            {
                                video.RequiresAuthentication = true;
                            }
                            else if (error.StartsWith("ERROR:"))
                            {
                                video.Failure = true;
                                video.FailureReason = error.Substring("ERROR: ".Length);
                            }
                        }, null, logger)
                    .WaitForExit();
            });
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

    public static class DictionaryExtensions
    {
        public static TValue Get<TKey, TValue>(this Dictionary<TKey, TValue> thiz, TKey key, TValue defaultValue)
        {
            if (!thiz.ContainsKey(key))
                thiz.Add(key, defaultValue);

            return thiz[key];
        }

        public static void Put<TKey, TValue>(this Dictionary<TKey, TValue> thiz, TKey key, TValue value)
        {
            if (thiz.ContainsKey(key))
                thiz[key] = value;
            else
                thiz.Add(key, value);
        }

        public static TValue Get<TValue>(this OrderedDictionary thiz, object key, object defaultValue)
        {
            if (!thiz.Contains(key))
                thiz.Add(key, defaultValue);

            return (TValue)thiz[key];
        }

        public static void Put(this OrderedDictionary thiz, object key, object value)
        {
            if (thiz.Contains(key))
                thiz[key] = value;
            else
                thiz.Add(key, value);
        }
    }
}
