using System;
using System.Globalization;
using System.Windows.Data;
using YouTube_Downloader_WPF.Classes;

namespace YouTube_Downloader_WPF.Converters
{
    [ValueConversion(typeof(long), typeof(string))]
    public class FileSizeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Helper.FormatFileSize((long)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
