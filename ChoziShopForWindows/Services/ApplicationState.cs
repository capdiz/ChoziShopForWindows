using ChoziShop.Data.Models;
using ChoziShopForWindows.models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.Services
{
    public sealed class ApplicationState : INotifyPropertyChanged
    {
        private static readonly Lazy<ApplicationState> _instance =
            new Lazy<ApplicationState>(() => new ApplicationState());

        public static ApplicationState Instance => _instance.Value;

        private ApplicationState() { }

        private DefaultUserAccount _defaultAccount;
        public DefaultUserAccount DefaultAccount
        {
            get => _defaultAccount;
            set
            {
                _defaultAccount = value;
                OnPropertyChanged(nameof(DefaultAccount));
            }
        }

        public Store CurrentStore { get; set; }

        public MerchantAccount StoreMerchantAccount { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
