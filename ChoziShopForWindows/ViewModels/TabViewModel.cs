using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ChoziShopForWindows.ViewModels
{
    public class TabViewModel : INotifyPropertyChanged
    {
        public  string Id { get; set; }
        public string Header { get; set; }
        public object Content { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
