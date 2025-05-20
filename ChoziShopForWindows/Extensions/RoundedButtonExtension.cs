using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ChoziShopForWindows.Extensions
{
    public class RoundedButtonExtension : DependencyObject
    {
        public static ImageSource GetImageSource(DependencyObject obj)
        {
            return (ImageSource)obj.GetValue(ImageSourceProperty);
        }

        public static void SetImageSource(DependencyObject obj, ImageSource value)
        {
            obj.SetValue(ImageSourceProperty, value);
        }

        private static readonly DependencyProperty imageSourceProperty = DependencyProperty.RegisterAttached(
            "ImageSource", typeof(ImageSource), typeof(RoundedButtonExtension), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public static DependencyProperty ImageSourceProperty => imageSourceProperty;

        public static void SetImageSize(DependencyObject obj, double value)
        {
            obj.SetValue(ImageSizeProperty, value);
        }

        public static readonly DependencyProperty ImageSizeProperty = DependencyProperty.RegisterAttached(
            "ImageSize", typeof(double), typeof(RoundedButtonExtension), new FrameworkPropertyMetadata(32.0, FrameworkPropertyMetadataOptions.AffectsMeasure |
                FrameworkPropertyMetadataOptions.AffectsRender));
    }
}
