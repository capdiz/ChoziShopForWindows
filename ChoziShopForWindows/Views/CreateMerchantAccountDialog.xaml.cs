using HandyControl.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChoziShopForWindows.Views
{
    /// <summary>
    /// Interaction logic for LoginDialog.xaml
    /// </summary>
    public partial class CreateMerchantAccountDialog : Border
    {
        public CreateMerchantAccountDialog()
        {
            InitializeComponent();
        }

        public void close()
        {
            ControlCommands.Close.Execute(true, this);
        }
    }
}
