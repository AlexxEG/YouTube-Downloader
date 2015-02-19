using System;
using System.Globalization;
using System.Windows.Data;

namespace YouTube_Downloader_WPF.Converters
{
    public class MultipleBoolsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (bool b in values)
            {
                if (!b) return false;
            }

            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("ConvertBack should never be called");
        }
    }
}