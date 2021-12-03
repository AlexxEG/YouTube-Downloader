using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using YouTube_Downloader_DLL.Enums;

namespace YouTube_Downloader_DLL.Classes
{
    public class Helper
    {
        /// <summary>
        /// Attempts to delete given file(s), ignoring exceptions for 10 tries, with 2 second delay between each try.
        /// </summary>
        /// <param name="files">The files to delete.</param>
        public static void DeleteFiles(params string[] files)
        {
            new Thread(delegate ()
            {
                var dict = new Dictionary<string, int>();
                var keys = new List<string>();

                foreach (string file in files)
                {
                    dict.Add(file, 0);
                    keys.Add(file);
                }

                while (dict.Count > 0)
                {
                    foreach (string key in keys)
                    {
                        try
                        {
                            if (File.Exists(key))
                                File.Delete(key);

                            // Remove file from dictionary since it either got deleted
                            // or it doesn't exist anymore.
                            dict.Remove(key);
                        }
                        catch
                        {
                            if (dict[key] == 10)
                            {
                                dict.Remove(key);
                            }
                            else
                            {
                                dict[key]++;
                            }
                        }
                    }

                    Thread.Sleep(2000);
                }
            }).Start();
        }

        public static async void DeleteFilesAsync(params string[] files)
        {
            await Task.Run(delegate
            {
                var dict = new Dictionary<string, int>();
                var keys = new List<string>();

                foreach (string file in files)
                {
                    dict.Add(file, 0);
                    keys.Add(file);
                }

                while (dict.Count > 0)
                {
                    foreach (string key in keys)
                    {
                        try
                        {
                            if (File.Exists(key))
                                File.Delete(key);

                            // Remove file from dictionary since it either got deleted
                            // or it doesn't exist anymore.
                            dict.Remove(key);
                        }
                        catch
                        {
                            if (dict[key] == 10)
                            {
                                dict.Remove(key);
                            }
                            else
                            {
                                dict[key]++;
                            }
                        }
                    }

                    Task.Delay(2000).Wait();
                }
            });
        }

        /// <summary>
        /// Returns a fixed URL, stripped of unnecessary invalid information. 
        /// </summary>
        /// <param name="url">The URL to fix.</param>
        public static string FixUrl(string url)
        {
            // Remove "Watch Later" information, causes error
            return url.Replace("&index=6&list=WL", string.Empty);
        }

        /// <summary>
        /// Returns a formatted string of the given file size.
        /// </summary>
        /// <param name="size">The file size as long to format.</param>
        public static string FormatFileSize(long size)
        {
            return string.Format(new FileSizeFormatProvider(), "{0:fs}", size);
        }

        /// <summary>
        /// Returns a formatted string of the given title, stripping illegal characters and replacing HTML entities with their actual character. (e.g. &quot; -> ')
        /// </summary>
        /// <param name="title">The title to format.</param>
        public static string FormatTitle(string title)
        {
            var replace = new Dictionary<string, string>()
            {
                {"|", "-"},
                {"&#39;", "'"},
                {"&quot;", "'"},
                {"&lt;", "("},
                {"&gt;", ")"},
                {"+", " "},
                {":", "-"},
                {"amp;", "&"}
            };

            var sb = new StringBuilder(title);

            // Remove illegal characters
            foreach (string s in new string[] { "/", @"\", "*", "?", "\"", "<", ">" })
                sb.Replace(s, string.Empty);

            foreach (var s in replace)
                sb.Replace(s.Key, s.Value);

            return sb.ToString().Trim();
        }

        /// <summary>
        /// Returns a formatted string of the video length.
        /// </summary>
        /// <param name="duration">The video duration as TimeSpan.</param>
        public static string FormatVideoLength(TimeSpan duration)
        {
            if (duration.Hours > 0)
                return string.Format("{0}:{1:00}:{2:00}", duration.Hours, duration.Minutes, duration.Seconds);
            else
                return string.Format("{0}:{1:00}", duration.Minutes, duration.Seconds);
        }

        /// <summary>
        /// Returns a formatted string of the video length.
        /// </summary>
        /// <param name="duration">The video duration as long.</param>
        public static string FormatVideoLength(long duration)
        {
            return FormatVideoLength(TimeSpan.FromSeconds(duration));
        }

        public static long GetDirectorySize(string directory)
        {
            return Directory
                    .GetFiles(directory, "*.*", SearchOption.AllDirectories)
                    .Sum(f => new FileInfo(f).Length);
        }

        public static string GetDirectorySizeFormatted(string directory)
        {
            return FormatFileSize(GetDirectorySize(directory));
        }

        /// <summary>
        /// Calculates the ETA.
        /// </summary>
        /// <param name="speed">The speed as bytes. (Bytes per second?)</param>
        /// <param name="totalBytes">The total amount of bytes.</param>
        /// <param name="downloadedBytes">The amount of downloaded bytes.</param>
        public static long GetETA(int speed, long totalBytes, long downloadedBytes)
        {
            if (speed == 0)
                return 0;

            // Get remaining bytes and divide by speed
            return (totalBytes - downloadedBytes) / speed;
        }

        /// <summary>
        /// Returns a long of the file size from given file in bytes.
        /// </summary>
        /// <param name="file">The file to get file size from.</param>
        public static long GetFileSize(string file)
        {
            if (!File.Exists(file))
                return 0;

            return new FileInfo(file).Length;
        }

        /// <summary>
        /// Returns an formatted string of the given file's size.
        /// </summary>
        /// <param name="size">The file to get and format file size for.</param>
        public static string GetFileSizeFormatted(string file)
        {
            return FormatFileSize(GetFileSize(file));
        }

        /// <summary>
        /// Returns the playlist id from given url.
        /// </summary>
        /// <param name="url">The url to get playlist id from.</param>
        public static string GetPlaylistId(string url)
        {
            var regex = new Regex(@"^(?:https?://)?(?:www.)?youtube.com/.*list=([0-9a-zA-Z\-_]*).*$");

            return regex.Match(url).Groups[1].Value;
        }

        /// <summary>
        /// Returns the highest quality audio format from the given VideoFormat.
        /// </summary>
        /// <param name="format">The format to get audio format from.</param>
        /// <returns>Returns <see langword="null"/> if no audio formats are found.</returns>
        public static VideoFormat GetAudioFormat(VideoFormat format)
        {
            // Find all audio only formats
            var audio = format.VideoInfo.Formats.FindAll(f => f.AudioOnly == true && f.Extension != "webm");

            // Return null if no audio is found
            if (audio.Count == 0)
                return null;

            // Return either the one with the highest audio bit rate, or the last found one
            return audio.OrderBy(a => a.AudioBitRate).Last();
        }

        /// <summary>
        /// Returns the preferred video format quality based on application setting.
        /// </summary>
        /// <param name="video">The video to get format from.</param>
        public static VideoFormat GetPreferredFormat(VideoInfo video, PreferredQuality preferredQuality)
        {
            /* Find a format based on user's preference.
             * 
             * Highest  : Self-explanatory
             * High     : 1080p
             * Medium   :  720p
             * Low      :  360p
             * Lowest   :  140p
             */
            var index = -1;
            var format = string.Empty;

            switch (preferredQuality)
            {
                // case PreferredQuality.Highest: - Do nothing here
                case PreferredQuality.High:
                    format = "1080"; break;
                case PreferredQuality.Medium:
                    format = "720"; break;
                case PreferredQuality.Low:
                    format = "360"; break;
                case PreferredQuality.Lowest:
                    format = "144"; break;
            }

            var qualities = Helper.GetVideoFormats(video);

            if (!string.IsNullOrEmpty(format))
                index = qualities.IndexOf(format);

            // If nothing found just return last
            if (index < 0) index = qualities.Length - 1;

            return qualities[index];
        }

        /// <summary>
        /// Returns a list of formats from the given VideoInfo, excluding vp9 and .webm videos.
        /// </summary>
        /// <param name="video">The video to get formats from.</param>
        public static VideoFormat[] GetVideoFormats(VideoInfo video)
        {
            // Skip audio only, audio + video & vp9 video formats
            return video.Formats
                .SkipWhile(x => x.AudioOnly || x.HasAudioAndVideo || x.VCodec.Contains("vp9") || !x.Extension.Contains("mp4"))
                .ToArray();
        }

        /// <summary>
        /// Returns true if the given url is a playlist YouTube url.
        /// </summary>
        /// <param name="url">The url to check.</param>
        public static bool IsPlaylist(string url)
        {
            var regex = new Regex(@"^(?:https?://)?(?:www.)?youtube.com/.*list=([0-9a-zA-Z\-_]*).*$");

            return regex.IsMatch(url);
        }

        /// <summary>
        /// Returns true if the given url is a valid and supported url.
        /// </summary>
        /// <param name="url">The url to check.</param>
        public static bool IsValidUrl(string url)
        {
            return IsValidTwitchUrl(url) || IsValidYouTubeUrl(url);
        }

        /// <summary>
        /// Returns true if the given url is a valid YouTube url.
        /// </summary>
        /// <param name="url">The url to check.</param>
        public static bool IsValidYouTubeUrl(string url)
        {
            if (!url.ToLower().Contains("www.youtube.com/watch?"))
                return false;

            var pattern = @"^(http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?$";

            return Regex.IsMatch(url, pattern, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Returns true if the given url is a valid Twitch url.
        /// </summary>
        /// <param name="url">The url to check.</param>
        public static bool IsValidTwitchUrl(string url)
        {
            var pattern = @"(https?:\/\/)?(www.)?twitch\.tv\/videos\/\d{9}(\?.*)?$";

            return Regex.IsMatch(url, pattern, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Creates a Process with the given arguments, then returns it.
        /// </summary>
        public static Process StartProcess(string fileName,
                                           string arguments,
                                           Action<Process, string> output,
                                           Action<Process, string> error,
                                           Dictionary<string, string> environmentVariables,
                                           OperationLogger logger)
        {
            var psi = new ProcessStartInfo(fileName, arguments)
            {
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = Common.GetLogsDirectory()
            };

            if (environmentVariables != null)
                foreach (KeyValuePair<string, string> pair in environmentVariables)
                    psi.EnvironmentVariables.Add(pair.Key, pair.Value);

            var process = new Process()
            {
                EnableRaisingEvents = true,
                StartInfo = psi
            };

            process.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
            {
                if (string.IsNullOrEmpty(e.Data))
                    return;

                logger?.LogLine(e.Data);
                output?.Invoke(process, e.Data);
            };
            process.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs e)
            {
                if (string.IsNullOrEmpty(e.Data))
                    return;

                logger?.LogLine(e.Data);
                error?.Invoke(process, e.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            return process;
        }
    }

    public static class FormatLeftTime
    {
        private static string[] TimeUnitsNames = { "Milli", "Sec", "Min", "Hour", "Day", "Month", "Year", "Decade", "Century" };
        private static int[] TimeUnitsValue = { 1000, 60, 60, 24, 30, 12, 10, 10 }; // Reference unit is milli

        public static string Format(long millis)
        {
            string format = string.Empty;

            for (int i = 0; i < TimeUnitsValue.Length; i++)
            {
                long y = millis % TimeUnitsValue[i];
                millis = millis / TimeUnitsValue[i];

                if (y == 0)
                    continue;

                format = y + " " + TimeUnitsNames[i] + " , " + format;
            }

            format = format.Trim(',', ' ');

            if (format == "")
                return "0 Sec";

            else return format;
        }
    }

    static class VideoFormatArrayExtensions
    {
        /// <summary>
        /// Returns the index of the given format in a array.
        /// </summary>
        /// <param name="thiz">The VideoFormat array to search.</param>
        /// <param name="format">The video format to find.</param>
        public static int IndexOf(this VideoFormat[] thiz, string format)
        {
            string pattern = @"^\d*x" + format + "$";
            Regex regex = new Regex(pattern);

            for (int i = 0; i < thiz.Length; i++)
            {
                VideoFormat f = thiz[i];

                // Skip .webm files
                if (f.Extension.Contains("webm"))
                    continue;

                if (regex.IsMatch(f.Format))
                    return i;
            }

            return -1;
        }
    }
}
