using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ChoziShopForWindows.Converters
{
    public class IntegerToStringConverter : IValueConverter
    {
        public string Value0 { get; set; } = string.Empty;
        public string Value1 { get; set; } = string.Empty; 
        public string Value2 { get; set; } = string.Empty;


        public string DefaultValue { get; set; } = string.Empty;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as int?) switch
            {
                0 => Value0,
                1 => Value1,
                2 => Value2,
                _ => DefaultValue
            };
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("ConvertBack is not supported for IntegerToStringConverter.");
        }
    }
}
