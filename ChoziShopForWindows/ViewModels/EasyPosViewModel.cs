using ChoziShopForWindows.Commands;
using ChoziShopForWindows.Views;
using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChoziShopForWindows.ViewModels
{
    public class EasyPosViewModel
    {
        private List<string> productNames;

        public EasyPosViewModel()
        {
            productNames = new List<string>();
            productNames.Add("Uganda");
            productNames.Add("Kenya");
            productNames.Add("Tanzania");

            ShowLoginDialogCommand = new RelayCommand(_ => ShowLoginDialog());
        }

        public List<string> ProductNames { get { return productNames; } }

        public ICommand ShowLoginDialogCommand { get; }

        private void ShowLoginDialog()
        {
            Dialog.Show(new LoginDialog());
        }
    }
}
