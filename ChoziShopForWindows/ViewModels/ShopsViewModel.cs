using ChoziShop.Data.Models;
using ChoziShopForWindows.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.ViewModels
{
    public class ShopsViewModel : INotifyPropertyChanged
    {
        private readonly IDataObjects _dataObjects;
        private List<Store> _stores;
        private string _shopsCountHeader;
        private string _storeName;
        public ShopsViewModel(IDataObjects dataObjects)
        {
            _dataObjects = dataObjects;
            LoadAvailableMerchantStores();
        }

        public List<Store> Stores
        {
            get { return _stores; }
            set { _stores = value;
                OnPropertyChanged();
            }
        }

        public string ShopsCountHeader
        {
            get { return _shopsCountHeader; }
            set
            {
                _shopsCountHeader = value;
                OnPropertyChanged();
            }
        }
      
        private async void LoadAvailableMerchantStores()
        {
            Stores = (List<Store>) await _dataObjects.Stores.GetAll();
            ShopsCountHeader = $"No of stores on ChoziShop: {Stores.Count}";
            for (int i = 0; i < Stores.Count; i++)
            {
                List<CategorySection> categorySections = await _dataObjects.GetStoreCategorySectionsAsync(Stores[i].InventoryId);
                List<Order> storeOrders = await _dataObjects.GetStoreOrdersAsync(Stores[i].OnlineStoreId);
                Stores[i].CategorySections = categorySections;                
                Stores[i].Orders = storeOrders;                
            }            
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
