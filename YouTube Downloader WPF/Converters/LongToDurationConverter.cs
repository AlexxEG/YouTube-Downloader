using System;
using System.Globalization;
using System.Windows.Data;

namespace YouTube_Downloader_WPF.Converters
{
    [ValueConversion(typeof(long), typeof(string))]
    public class LongToDurationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str = string.Empty;
            TimeSpan duration = TimeSpan.FromSeconds((long)value);

            if (duration.Hours > 0)
                str = string.Format("{0}:{1:00}:{2:00}", duration.Hours, duration.Minutes, duration.Seconds);
            else
                str = string.Format("{0}:{1:00}", duration.Minutes, duration.Seconds);

            return str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
