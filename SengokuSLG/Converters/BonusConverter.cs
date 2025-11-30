using System;
using System.Globalization;
using System.Windows.Data;

namespace SengokuSLG.Converters
{
    public class BonusConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 || values[0] == null || values[1] == null)
                return "0";

            if (int.TryParse(values[0].ToString(), out int advisorStat) && 
                int.TryParse(values[1].ToString(), out int playerStat))
            {
                int diff = Math.Max(advisorStat - playerStat, 0);
                int bonus = (int)(diff * 0.4);
                return bonus > 0 ? $"+{bonus}" : "0";
            }

            return "0";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FinalValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 || values[0] == null || values[1] == null)
                return "0";

            if (int.TryParse(values[0].ToString(), out int advisorStat) && 
                int.TryParse(values[1].ToString(), out int playerStat))
            {
                int diff = Math.Max(advisorStat - playerStat, 0);
                int bonus = (int)(diff * 0.4);
                return (playerStat + bonus).ToString();
            }

            return values[1].ToString();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
