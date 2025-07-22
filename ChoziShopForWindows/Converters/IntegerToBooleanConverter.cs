using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ChoziShopForWindows.Converters
{
    public class IntegerToBooleanConverter : IValueConverter
    {
        // Conversion modes
        public enum ConversionMode
        {
            ZeroIsTrue,
            ZeroIsFalse,
            OneIsTrue,
            OneIsFalse,
            NonZeroIsTrue
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is int intValue)
            {
                if (parameter is string modeString && Enum.TryParse(modeString, out ConversionMode mode))
                {
                    switch (mode)
                    {
                        case ConversionMode.ZeroIsTrue:
                            return intValue == 0;
                        case ConversionMode.ZeroIsFalse:
                            return intValue != 0;
                        case ConversionMode.OneIsTrue:
                            return intValue == 1;
                        case ConversionMode.OneIsFalse:
                            return intValue != 1;
                        case ConversionMode.NonZeroIsTrue:
                            return intValue != 0;
                    }
                }
                // Default behavior
                return intValue != 0;
            }
            return false; // or DependencyProperty.UnsetValue if you prefer
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                var mode = GetConversionMode(parameter);
                switch (mode)
                {
                    case ConversionMode.ZeroIsTrue:
                        return boolValue ? 0 : 1;
                    case ConversionMode.ZeroIsFalse:
                        return boolValue ? 1 : 0;
                    case ConversionMode.OneIsTrue:
                        return boolValue ? 1 : 0;
                    case ConversionMode.OneIsFalse:
                        return boolValue ? 0 : 1;
                    case ConversionMode.NonZeroIsTrue:
                        return boolValue ? 1 : 0;
                }

            }
            return false;

        }

        private ConversionMode GetConversionMode(object parameter)
        {
            if (parameter is ConversionMode mode) return mode;
            if (parameter is string modeString && Enum.TryParse(modeString, out ConversionMode parsedMode))
            {
                return parsedMode;
            }
            return ConversionMode.ZeroIsTrue;
        }
    }
}
