using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace SengokuSLG.Converters
{
    public class BooleanToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#c73940")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f8f3e7"));
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class BooleanToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = (bool)value;
            if (parameter != null && parameter is string paramStr)
            {
                var parts = paramStr.Split('|');
                if (parts.Length == 2)
                {
                    return boolValue ? parts[0] : parts[1];
                }
            }
            return boolValue ? "公開" : "未公開";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class BooleanToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Brushes.White : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2b2b2b"));
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = (bool)value;
            if (parameter != null && parameter.ToString() == "Inverse")
            {
                boolValue = !boolValue;
            }
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }
    }

    public class DisclosureConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isExpanded)
            {
                return isExpanded ? "▼" : "▶";
            }
            return "▶";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AbilityDisclosureConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is int abilityValue && values[1] is bool isDisclosed)
            {
                return isDisclosed ? abilityValue.ToString() : "?";
            }
            return "?";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BonusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int bonus)
            {
                if (bonus > 0) return $"+{bonus}";
                if (bonus < 0) return bonus.ToString();
                return "±0";
            }
            return "±0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FinalValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is int baseValue && values[1] is int bonus)
            {
                return baseValue + bonus;
            }
            return 0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RankToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Models.Rank rank)
            {
                return rank switch
                {
                    Models.Rank.Juboku => "従僕",
                    Models.Rank.Toshi => "徒士",
                    Models.Rank.Bajoshu => "馬上衆",
                    Models.Rank.Kogashira => "小頭",
                    Models.Rank.Kumigashira => "組頭",
                    Models.Rank.AshigaruDaisho => "足軽大将",
                    Models.Rank.Jidaisho => "侍大将",
                    Models.Rank.Busho => "部将",
                    _ => "不明"
                };
            }
            return "不明";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RankToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Models.Rank rank)
            {
                return rank switch
                {
                    Models.Rank.Juboku => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#888888")),      // 従僕: グレー
                    Models.Rank.Toshi => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1d3557")),       // 徒士: 紺藍
                    Models.Rank.Bajoshu => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4a5899")),     // 馬上衆: 青紫
                    Models.Rank.Kogashira => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1976d2")),   // 小頭: 青
                    Models.Rank.Kumigashira => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2d7a3e")), // 組頭: 緑
                    Models.Rank.AshigaruDaisho => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d97706")), // 足軽大将: 橙
                    Models.Rank.Jidaisho => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#c73940")),       // 侍大将: 赤
                    Models.Rank.Busho => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8b4513")),    // 部将: 茶
                    _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#888888"))
                };
            }
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#888888"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IntToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                return intValue > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AdvisorBonusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Always show "+?" when there is an advisor (value is not null and >= 0)
            if (value is int)
            {
                return "+?";
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IndustryTypeToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Models.IndustryType type)
            {
                return type switch
                {
                    Models.IndustryType.Agriculture => "農業",
                    Models.IndustryType.Smithing => "鍛冶",
                    Models.IndustryType.Weaving => "織物",
                    Models.IndustryType.Brewing => "醸造",
                    Models.IndustryType.Mining => "鉱山",
                    _ => type.ToString()
                };
            }
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class ProductCategoryToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Models.ProductCategory category)
            {
                return category switch
                {
                    Models.ProductCategory.Food => "食料",
                    Models.ProductCategory.Material => "資材",
                    Models.ProductCategory.Weapon => "武具",
                    Models.ProductCategory.Luxury => "贅沢品",
                    _ => category.ToString()
                };
            }
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class CreditRankToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Models.CreditRank rank)
            {
                return rank switch
                {
                    Models.CreditRank.Acquaintance => "面識",
                    Models.CreditRank.Regular => "常連",
                    Models.CreditRank.VIP => "上客",
                    _ => rank.ToString()
                };
            }
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class MerchantTierToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Models.MerchantTier tier)
            {
                return tier switch
                {
                    Models.MerchantTier.Traveling => "行商",
                    Models.MerchantTier.Town => "街商人",
                    Models.MerchantTier.Regional => "地方商人",
                    Models.MerchantTier.City => "都市商人",
                    _ => tier.ToString()
                };
            }
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class RoadLevelToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Models.RoadLevel level)
            {
                return level switch
                {
                    Models.RoadLevel.Road => "街道",
                    Models.RoadLevel.Highway => "本街道",
                    _ => level.ToString()
                };
            }
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class VillageTypeToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Models.VillageType type)
            {
                return type switch
                {
                    Models.VillageType.Village => "村",
                    Models.VillageType.Town => "町",
                    _ => type.ToString()
                };
            }
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
