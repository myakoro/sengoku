using System;
using System.Globalization;
using System.Windows.Data;
using SengokuSLG.Models;

namespace SengokuSLG.Converters
{
    public class RankToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Rank rank)
            {
                switch (rank)
                {
                    case Rank.Juboku: return "従僕";
                    case Rank.Toshi: return "徒士";
                    case Rank.Kumigashira: return "組頭";
                    case Rank.Busho: return "武将";
                    case Rank.Jidaisho: return "侍大将";
                    default: return rank.ToString();
                }
            }
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
