using System;
using System.Globalization;
using System.Windows.Data;

namespace YouTube_Downloader_WPF.Converters
{
    [ValueConversion(typeof(bool), typeof(string))]
    public class PauseResumeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((bool)value) ? "Resume" : "Pause";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("ConvertBack should never be called");
        }
    }
}
