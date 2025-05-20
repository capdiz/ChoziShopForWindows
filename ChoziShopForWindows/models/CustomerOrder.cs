using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.models
{
    public class CustomerOrder : INotifyPropertyChanged
    {
        private decimal totalAmountCents;
        public decimal TotalAmountCents
        {
            get { return totalAmountCents; }
            set
            {
                totalAmountCents = value;
                OnPropertyChanged(nameof(TotalAmountCents));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
