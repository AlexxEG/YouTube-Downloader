using System;

namespace YouTube_Downloader_DLL
{
    public class FileSizeFormatProvider : IFormatProvider, ICustomFormatter
    {
        private const string FileSizeFormat = "fs", SpeedFormat = "s";
        private const Decimal OneKiloByte = 1024M;
        private const Decimal OneMegaByte = OneKiloByte * 1024M;
        private const Decimal OneGigaByte = OneMegaByte * 1024M;

        private static string DefaultFormat(string format, object arg, IFormatProvider formatProvider)
        {
            IFormattable formattableArg = arg as IFormattable;

            if (formattableArg != null)
            {
                return formattableArg.ToString(format, formatProvider);
            }

            return arg.ToString();
        }

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (format == null || (!format.StartsWith(FileSizeFormat) && !format.StartsWith(SpeedFormat)))
            {
                return DefaultFormat(format, arg, formatProvider);
            }

            if (arg is string)
            {
                return DefaultFormat(format, arg, formatProvider);
            }

            decimal size;

            try
            {
                size = Convert.ToDecimal(arg);
            }
            catch (InvalidCastException)
            {
                return DefaultFormat(format, arg, formatProvider);
            }

            string suffix;

            if (size > OneGigaByte)
            {
                size /= OneGigaByte;
                suffix = "GB";
            }
            else if (size > OneMegaByte)
            {
                size /= OneMegaByte;
                suffix = "MB";
            }
            else if (size > OneKiloByte)
            {
                size /= OneKiloByte;
                suffix = "KB";
            }
            else
            {
                suffix = "Bytes";
            }

            if (format.StartsWith(SpeedFormat))
                suffix += "/s";

            int postion = format.StartsWith(SpeedFormat) ? SpeedFormat.Length : FileSizeFormat.Length;
            string precision = format.Substring(postion);

            if (string.IsNullOrEmpty(precision))
                precision = "2";

            return string.Format("{0:N" + precision + "}{1}", size, " " + suffix);

        }

        public object GetFormat(Type formatType)
        {
            return formatType == typeof(ICustomFormatter) ? this : null;
        }
    }
}