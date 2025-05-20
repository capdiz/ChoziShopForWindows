using ChoziShop.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.models
{
    public class SelectedCategoryProduct : INotifyPropertyChanged
    {
        private int _itemNo;
        private string _itemName;
        private int _units;
        private CategoryProduct _categoryProduct;
        private decimal _price;
        private decimal _totalPrice;
        

        public int ItemNo
        {
            get { return _itemNo; }
            set
            {
                _itemNo = value;
                OnPropertyChanged();
            }
        }   
        public string ItemName
        {
            get { return _itemName; }
            set
            {
                _itemName = value;
                OnPropertyChanged();
            }
        }   

        public int UnitCount { 
            get { return _units; }
            set
            {
                _units = value;
                OnPropertyChanged();
            }
        }
        public CategoryProduct CategoryProduct
        {
            get { return _categoryProduct; }
            set
            {
                _categoryProduct = value;
                OnPropertyChanged();
            }
        }


        public decimal UnitPrice { 
            get { return _price; }
            set
            {
                _price = value;
                OnPropertyChanged();
            }
        }
        public decimal TotalPrice { 
            get { return _totalPrice; }
            set
            {
                _totalPrice = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
