using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using YouTube_Downloader.Enums;
using YouTube_Downloader.Properties;

namespace YouTube_Downloader.Classes
{
    public class Helper
    {
        public const int PreferedQualityHighest = 0;
        public const int PreferedQualityMedium = 1;
        public const int PreferedQualityLow = 2;

        /// <summary>
        /// Attempts to delete given file(s), ignoring exceptions for 10 tries, with 2 second delay between each try.
        /// </summary>
        /// <param name="files">The files to delete.</param>
        public static void DeleteFiles(params string[] files)
        {
            new Thread(delegate()
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

        /// <summary>
        /// Returns a fixed URL, stripped of unnecessary invalid information. 
        /// </summary>
        /// <param name="url">The URL to fix.</param>
        public static string FixUrl(string url)
        {
            // Remove "Watch Later" information, causes error
            url = url.Replace("&index=6&list=WL", "");

            return url;
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
            string[] illegalCharacters = new string[] { "/", @"\", "*", "?", "\"", "<", ">" };

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

            var sb = new System.Text.StringBuilder(title);

            foreach (string s in illegalCharacters)
                sb.Replace(s, string.Empty);

            foreach (KeyValuePair<string, string> s in replace)
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

            long remainBytes = totalBytes - downloadedBytes;

            return remainBytes / speed;
        }

        /// <summary>
        /// Returns a formatted string of file size from given file.
        /// </summary>
        /// <param name="file">The file to get file size from.</param>
        public static string GetFileSize(string file)
        {
            FileInfo info = new FileInfo(file);

            return Helper.FormatFileSize(info.Length);
        }

        /// <summary>
        /// Returns the playlist id from given url.
        /// </summary>
        /// <param name="url">The url to get playlist id from.</param>
        public static string GetPlaylistId(string url)
        {
            Regex regex = new Regex(@"^(?:https?:\/\/)?(?:www.)?youtube.com\/.*list=([0-9a-zA-Z\-\_]*).*$");

            return regex.Match(url).Groups[1].Value;
        }

        /// <summary>
        /// Returns the highest quality DASH audio format from the given VideoFormat.
        /// </summary>
        /// <param name="format">The format to get audio format from.</param>
        public static VideoFormat GetAudioFormat(VideoFormat format)
        {
            List<VideoFormat> audio = new List<VideoFormat>();

            // Add all audio only formats
            audio.AddRange(format.VideoInfo.Formats.FindAll(f => f.AudioOnly == true && f.FormatType == format.FormatType));

            // Return null if no audio is found
            if (audio.Count == 0)
                return null;

            // Return either the one with the highest audio bit rate, or the last found one
            return audio.OrderBy(a => a.AudioBitRate).Last();
        }

        /// <summary>
        /// Returns the prefered video format quality based on application setting.
        /// </summary>
        /// <param name="video">The video to get format from.</param>
        /// <param name="dash">True to only get DASH formats.</param>
        public static VideoFormat GetPreferedFormat(VideoInfo video, bool dash)
        {
            VideoFormat[] qualities = Helper.GetVideoFormats(video, dash);

            /* Find a format based on user's preference.
             * 
             * Highest  : Self-explanatory
             * Medium   : 720p or highest
             * Low      : 360p or highest
             */

            int index = -1;

            switch (Settings.Default.PreferedQualityPlaylist)
            {
                case PreferedQualityMedium:
                    if ((index = qualities.IndexOf("720", dash)) > -1)
                    {
                        return qualities[index];
                    }
                    break;
                case PreferedQualityLow:
                    if ((index = qualities.IndexOf("360", dash)) > -1)
                    {
                        return qualities[index];
                    }
                    break;
            }

            if (!(index > -1)) index = qualities.Length - 1;

            return qualities[index];
        }

        /// <summary>
        /// Returns a list of formats from the given VideoInfo, excluding audio & DASH formats if dash is false.
        /// </summary>
        /// <param name="video">The video to get formats from.</param>
        /// <param name="dash">True to only get DASH formats.</param>
        public static VideoFormat[] GetVideoFormats(VideoInfo video, bool dash)
        {
            var formats = new List<VideoFormat>();

            foreach (VideoFormat format in video.Formats)
            {
                // Skip audio only formats
                if (format.AudioOnly)
                    continue;

                // Exclude DASH videos if dash is false.
                if (!dash && format.FormatType != FormatType.Normal)
                    continue;

                // Only include .mp4 DASH videos if dash is true.
                if (dash && format.FormatType == FormatType.Normal || !format.Extension.Contains("mp4"))
                    continue;

                formats.Add(format);
            }

            return formats.ToArray();
        }

        /// <summary>
        /// Returns true if the given url is a playlist YouTube url.
        /// </summary>
        /// <param name="url">The url to check.</param>
        public static bool IsPlaylist(string url)
        {
            Regex regex = new Regex(@"^(?:https?:\/\/)?(?:www.)?youtube.com\/.*list=([0-9a-zA-Z\-\_]*).*$");

            return regex.IsMatch(url);
        }

        /// <summary>
        /// Returns true if the given url is a valid YouTube url.
        /// </summary>
        /// <param name="url">The url to check.</param>
        public static bool IsValidYouTubeUrl(string url)
        {
            if (!url.ToLower().Contains("www.youtube.com/watch?"))
                return false;

            string pattern = @"^(http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?$";
            Regex regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

            return regex.IsMatch(url);
        }
    }

    static class FormatLeftTime
    {
        private static string[] TimeUnitsNames = { "Milli", "Sec", "Min", "Hour", "Day", "Month", "Year", "Decade", "Century" };
        private static int[] TimeUnitsValue = { 1000, 60, 60, 24, 30, 12, 10, 10 }; // Reference unit is milli

        public static string Format(long millis)
        {
            string format = "";

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
        /// <param name="dash">True to find DASH format, false if otherwise.</param>
        public static int IndexOf(this VideoFormat[] thiz, string format, bool dash)
        {
            string pattern = @"^\d*\s-\s\d*x" + format + "$";
            string patternDASH = @"^\d*\s-\s\d*x" + format + @"\s\(DASH video\)$";
            Regex regex = new Regex(dash ? patternDASH : pattern);

            for (int i = 0; i < thiz.Length; i++)
            {
                VideoFormat f = thiz[i];

                // Skip .webm files
                if (f.Extension.Contains("webm"))
                    continue;

                // Ignore '(DASH video)' suffix
                if (regex.IsMatch(f.Format))
                    return i;
            }

            return -1;
        }
    }
}
