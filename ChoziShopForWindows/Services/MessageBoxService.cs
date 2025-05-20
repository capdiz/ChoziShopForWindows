using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ChoziShopForWindows.Services
{
    public class MessageBoxService : IDialogService
    {
        public MessageBoxResult ShowMessageBox(string message, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            return MessageBox.Show(message, caption, button, icon);
        }
    }
    
}
