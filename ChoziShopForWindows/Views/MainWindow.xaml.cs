using System.Resources;
using System.Text;
using System.Windows;
using HandyControl.Controls;
using HandyControl.Tools;
using HandyControl.Tools.Extension;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ChoziShopForWindows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    
    public MainWindow()
    {
        InitializeComponent();

        InitializeResourceDictionary();      
    }


    private void InitializeResourceDictionary() {

        // Theme
        ResourceDictionary theme = new ResourceDictionary
        {
            Source = new Uri("pack://application:,,,/Resources/Themes/BaseLight.xaml")
        };

        Resources.MergedDictionaries.Add(theme);
        // Color
        ResourceDictionary color = new ResourceDictionary
        {
            Source = new Uri("pack://application:,,,/Resources/Style/Primary/Primary.xaml")
        };
        Resources.MergedDictionaries.Add(color);

        // String values
        ResourceDictionary values = new ResourceDictionary {
            Source = new Uri("pack://application:,,,/Resources/StringValues.xaml")
        };
        Resources.MergedDictionaries.Add(values);

        // Custom colours
        ResourceDictionary customColor = new ResourceDictionary()
        {
            Source = new Uri("pack://application:,,,/Resources/Style/CustomColours.xaml")
        };
        Resources.MergedDictionaries.Add(customColor);

        // Converter resources
        ResourceDictionary converters = new ResourceDictionary()
        {
            Source = new Uri("pack://application:,,,/Resources/Converters/ChoziShopConverters.xaml")
        };
        Resources.MergedDictionaries.Add(converters);

        // RoundedButton extension
        ResourceDictionary roundedButton = new ResourceDictionary()
        {
            Source = new Uri("pack://application:,,,/Resources/Style/RoundedButtonExtension.xaml")
        };
        Resources.MergedDictionaries.Add(roundedButton);
    }


}