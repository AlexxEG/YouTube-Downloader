using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
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

        public static string FormatFileSize(long size)
        {
            return string.Format(new FileSizeFormatProvider(), "{0:fs}", size);
        }

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

        public static string FormatVideoLength(TimeSpan duration)
        {
            if (duration.Hours > 0)
                return string.Format("{0}:{1:00}:{2:00}", duration.Hours, duration.Minutes, duration.Seconds);
            else
                return string.Format("{0}:{1:00}", duration.Minutes, duration.Seconds);
        }

        public static string FormatVideoLength(long duration)
        {
            return FormatVideoLength(TimeSpan.FromSeconds(duration));
        }

        public static long GetETA(int speed, long totalBytes, long downloadedBytes)
        {
            if (speed == 0)
                return 0;

            long remainBytes = totalBytes - downloadedBytes;

            return remainBytes / speed;
        }

        public static string GetFileSize(string file)
        {
            FileInfo info = new FileInfo(file);

            return Helper.FormatFileSize(info.Length);
        }

        public static string GetPlaylistId(string url)
        {
            Regex regex = new Regex("https?://www.youtube.com/.*[playlist|watch].*list=(\\w+)");

            return regex.Match(url).Groups[1].Value;
        }

        public static VideoFormat GetAudioFormat(VideoInfo video)
        {
            foreach (VideoFormat f in video.Formats)
            {
                if (f.Format.Contains("audio only (DASH audio)"))
                    return f;
            }
            return null;
        }

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

        public static VideoFormat[] GetVideoFormats(VideoInfo video, bool dash)
        {
            var formats = new List<VideoFormat>();
            Regex regex = new Regex(dash ? "(144|240|360|480|720|1080|1440|2160)p" : @"^\d*\s*-\s*\d+x\d+$");

            foreach (VideoFormat format in video.Formats)
            {
                // Exclude DASH videos if dash is false.
                if (!dash && format.DASH)
                {
                    continue;
                }
                // Only include .mp4 DASH videos if dash is true.
                else if (dash && !format.DASH || !format.Extension.Contains("mp4"))
                {
                    continue;
                }

                if (regex.IsMatch(format.Format))
                {
                    formats.Add(format);
                }
            }

            return formats.ToArray();
        }

        public static bool IsPlaylist(string url)
        {
            Regex regex = new Regex("https?://www.youtube.com/.*[playlist|watch].*list=(\\w+)");

            return regex.IsMatch(url);
        }

        public static bool IsValidUrl(string url)
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
        private static int[] TimeUnitsValue = { 1000, 60, 60, 24, 30, 12, 10, 10 };//refrernce unit is milli

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
        public static int IndexOf(this VideoFormat[] thiz, string format, bool dash)
        {
            Regex regex = new Regex(dash ? "^.* - " + format + "p.*$" : @"^\d*\s*-\s*\d+x" + format + "$");

            for (int i = 0; i < thiz.Length; i++)
            {
                VideoFormat f = thiz[i];

                /* Ignore '(DASH video)' suffix. */
                if (regex.IsMatch(f.Format))
                    return i;
            }

            return -1;
        }
    }
}
