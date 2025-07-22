using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ChoziShopForWindows.Converters
{
    public class PaymentStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return string.Empty;
            if (value is int statusValue)
            {
                return statusValue switch
                {
                    0 => "Pending",
                    1 => "Successful",
                    2 => "Failed",
                    _ => "Unknown Status" // Fallback for unexpected values
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
