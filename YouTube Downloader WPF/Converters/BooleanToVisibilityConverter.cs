using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace YouTube_Downloader_WPF.Converters
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanToVisibilityConverter : MarkupExtension, IValueConverter
    {
        public bool Collapse { get; set; } = false;
        public bool Reverse { get; set; } = false;

        private Visibility HiddenVisibility
        {
            get
            {
                return Collapse ? Visibility.Collapsed : Visibility.Hidden;
            }
        }

        private Visibility Visibility
        {
            get
            {
                return Visibility.Visible;
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool bValue = false;
            if (value is bool)
            {
                bValue = (bool)value;
            }
            else if (value is Nullable<bool>)
            {
                Nullable<bool> tmp = (Nullable<bool>)value;
                bValue = tmp.HasValue ? tmp.Value : false;
            }

            if (!this.Reverse)
                return bValue ? this.Visibility : this.HiddenVisibility;
            else
                return bValue ? this.HiddenVisibility : this.Visibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility)
            {
                var visibility = (Visibility)value;
                return !this.Reverse ? visibility == Visibility.Visible : visibility == Visibility.Collapsed || visibility == Visibility.Hidden;
            }
            else
            {
                return false;
            }
        }
    }
}