using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ChoziShopForWindows.Converters
{
    public class BoolToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string paths)
            {
                var imagePaths = paths.Split(';');
                var imagePath = boolValue ? imagePaths[0] : imagePaths[1];
                return new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute));
            }
            return null;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
