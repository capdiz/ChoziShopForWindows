using ChoziShop.Data.Models;
using ChoziShopForWindows.ViewModels;
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
    /// Interaction logic for PleaseWaitDialog.xaml
    /// </summary>
    public partial class AddKeeperDialog : Border
    {
        
        public AddKeeperDialog(ShopKeepersViewModel shopKeepersViewModel)
        {
            InitializeComponent();
            DataContext = shopKeepersViewModel;
        }


        public void close()
        {
            ControlCommands.Close.Execute(true, this);
        }
    }
}
