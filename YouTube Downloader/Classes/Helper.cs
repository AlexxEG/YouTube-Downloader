using System.Text.RegularExpressions;

namespace YouTube_Downloader.Classes
{
    public class Helper
    {
        public static string FormatFileSize(long size)
        {
            return string.Format(new FileSizeFormatProvider(), "{0:fs}", size);
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
}
