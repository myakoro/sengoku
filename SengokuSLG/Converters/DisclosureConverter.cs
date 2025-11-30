using System;
using System.Globalization;
using System.Windows.Data;
using SengokuSLG.Models;

namespace SengokuSLG.Converters
{
    public class DisclosureConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2) return "???";

            var value = values[0]; // The ability value (int)
            var isDisclosed = values[1] as bool? ?? false; // The disclosure flag

            if (isDisclosed)
            {
                return value.ToString();
            }
            return "???";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
