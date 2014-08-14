using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace YouTube_Downloader.Classes
{
    public class Helper
    {
        public const int PreferedQualityHighest = 0;
        public const int PreferedQualityMedium = 1;
        public const int PreferedQualityLow = 2;

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

        public static VideoFormat GetPreferedFormat(VideoInfo video)
        {
            VideoFormat[] qualities = Helper.GetVideoFormats(video);

            /* 
             * Find a format based on user's preference.
             * 
             * Highest  : Self-explanatory
             * Medium   : 720p or highest
             * Low      : 360p or highest
             */

            int index = -1;

            switch (SettingsEx.PreferedQuality)
            {
                case PreferedQualityMedium:
                    if ((index = qualities.IndexOf("720p")) > -1)
                    {
                        return qualities[index];
                    }
                    break;
                case PreferedQualityLow:
                    if ((index = qualities.IndexOf("360p")) > -1)
                    {
                        return qualities[index];
                    }
                    break;
            }

            if (!(index > -1)) index = qualities.Length - 1;

            return qualities[index];
        }

        public static VideoFormat[] GetVideoFormats(VideoInfo video)
        {
            var formats = new List<VideoFormat>();

            foreach (VideoFormat format in video.Formats)
            {
                if (!format.Extension.Contains("mp4") || !format.Format.Contains("DASH"))
                    continue;

                if (Regex.IsMatch(format.Format, "(144|240|360|480|720|1080|1440|2160)p"))
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

    public static class FormatLeftTime
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
        public static int IndexOf(this VideoFormat[] thiz, string format)
        {
            for (int i = 0; i < thiz.Length; i++)
            {
                VideoFormat f = thiz[i];

                /* Ignore '(DASH video)' suffix. */
                if (Regex.IsMatch(f.Format, "^.* - " + format + ".*$"))
                    return i;
            }

            return -1;
        }
    }
}
