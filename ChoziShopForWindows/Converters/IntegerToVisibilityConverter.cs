using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ChoziShopForWindows.Converters
{
    public class IntegerToVisibilityConverter : IValueConverter
    {
        public enum ConversionMode
        {
            ZeroIsVisible,
            ZeroIsCollapsed,
            OneIsVisible,
            OneIsCollapsed,
            NonZeroIsVisible,
            NonZeroIsCollapsed
        }

        public int CompareValue { get; set; } = 0;

        public Visibility FalseVisibility { get; set; } = Visibility.Collapsed;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is int intValue)
            {
                if (parameter is string modeString && Enum.TryParse(modeString, out ConversionMode mode))
                {
                    switch (mode)
                    {
                        case ConversionMode.ZeroIsVisible:
                            return intValue == CompareValue ? Visibility.Visible : FalseVisibility;
                        case ConversionMode.ZeroIsCollapsed:
                            return intValue == CompareValue ? Visibility.Collapsed : FalseVisibility;
                        case ConversionMode.OneIsVisible:
                            return intValue == 1 ? Visibility.Visible : FalseVisibility;
                        case ConversionMode.OneIsCollapsed:
                            return intValue == 1 ? Visibility.Collapsed : FalseVisibility;
                        case ConversionMode.NonZeroIsVisible:
                            return intValue != 0 ? Visibility.Visible : FalseVisibility;
                        case ConversionMode.NonZeroIsCollapsed:
                            return intValue != 0 ? Visibility.Collapsed : FalseVisibility;
                    }
                }
                // Default behavior
                return intValue != 0 ? Visibility.Visible : FalseVisibility;
            }
            return FalseVisibility; // or DependencyProperty.UnsetValue if you prefer
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("ConvertBack is not supported for IntegerToVisibilityConverter.");
        }

      
    }
}
