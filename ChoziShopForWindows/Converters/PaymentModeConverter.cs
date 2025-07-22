using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ChoziShopForWindows.Converters
{
   public class PaymentModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return string.Empty;
            if (value is int strValue)
            {
                return strValue switch
                {
                    0 => "Cash",
                    1 => "Mobile Money",
                    2 => "Debit Card",
                    4 => "Pay later",
                    _ => string.Empty // Fallback to original value if no match
                };
            }
            return value.ToString(); // Return original value if not an int
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
  
}
