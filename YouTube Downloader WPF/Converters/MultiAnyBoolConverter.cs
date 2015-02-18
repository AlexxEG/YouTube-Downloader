using System;
using System.Globalization;
using System.Windows.Data;

namespace YouTube_Downloader_WPF.Converters
{
    public class MultiAnyBoolConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (bool b in values)
            {
                if (b) return true;
            }

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("ConvertBack should never be called");
        }
    }
}
