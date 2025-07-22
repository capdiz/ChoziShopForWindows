using ChoziShop.Data.Models;
using ChoziShopForWindows.Data;
using ChoziShopForWindows.models;
using ChoziShopForWindows.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ChoziShopForWindows.ViewModels
{
    // Used to view inventory log details of a given item
    public class InventoryTrackerViewModel : INotifyPropertyChanged
    {
        private readonly IDataObjects dataObjects;
        private StockItem _stockItem;
        private List<InventoryTracker> _inventoryTrackers;
        private decimal _purchasePrice;
        private int _units;
        private decimal _customerPurchasePrice;
        
        private ConcurrentDictionary<long, string> _userLookupCache=new ConcurrentDictionary<long, string>();

        public EventHandler<bool> RestockCompleteHandler;


        public InventoryTrackerViewModel(IDataObjects dataObjects, StockItem stockItem)
        {
            this.dataObjects = dataObjects;
            _=LoadUserAccounts();
            LoadInventoryLog(stockItem);
            Debug.WriteLine("Count of log: "+InventoryTrackers.Count);  
            StockItem = stockItem;            
            EditProductCommand = new Commands.RelayCommand(_ => RestockItem());
        }
        public StockItem StockItem
        {
            get { return _stockItem; }
            set
            {
                _stockItem = value;
                OnPropertyChanged();
            }
        }

        public decimal PurchasePrice
        {
            get { return _purchasePrice; }
            set
            {
                _purchasePrice = value;
                OnPropertyChanged();
            }
        }

        public int Units
        {
            get { return _units; }
            set
            {
                _units = value;
                OnPropertyChanged();
            }
        }

        public decimal CustomerPurchasePrice
        {
            get { return _customerPurchasePrice; }
            set
            {
                _customerPurchasePrice = value;
                OnPropertyChanged();
            }
        }

        public string RestockItemHeader
        {
            get
            {
                return $"Restock {StockItem.ItemName}";
            }
        }

        public string EditProductHeader
        {
            get
            {
                return $"Edit more {StockItem.ItemName} Details.";
            }
        }

        public string StockItemName
        {
            get { return $"{StockItem.ItemName}'s Inventory Log History"; }
        }

        public List<InventoryTracker> InventoryTrackers
        {
            get { return _inventoryTrackers; }
            set
            {
                _inventoryTrackers = value;
                OnPropertyChanged();
            }
        }

   
        public ICommand EditProductCommand { get; }

        private async Task RestockItem()
        {
            if (PurchasePrice > 0 && Units > 0 &&
                CustomerPurchasePrice > 0)
            {
                var categoryProduct = await dataObjects.CategoryProducts.GetById(StockItem.Id);
                if (categoryProduct != null)
                {
                    CategoryProduct product = categoryProduct;
                    if (product.OnlineCategorySectionId == 0)
                    {
                        product.OnlineCategorySectionId = await dataObjects.GetCategorySectionOnlineId(product.CategorySectionId);
                    }
                    product.Units = Units;
                    product.UnitCost = CustomerPurchasePrice;
                    product.UpdatedAt = DateTime.Now;
                    var savedProduct = dataObjects.CategoryProducts.UpdateAndReturnEntityAsync(product);
                    if (savedProduct != null)
                    {
                        var defaultUserAccount = ApplicationState.Instance.DefaultAccount;
                        var currentStore = ApplicationState.Instance.CurrentStore; 
                        Debug.WriteLine("User account: "+defaultUserAccount.FullName);
                        if (defaultUserAccount != null && currentStore != null)
                        {
                            string accountType = defaultUserAccount.AccountType == 1 ? "merchant" : "keeper";
                            InventoryTracker inventoryTracker = new InventoryTracker
                            {
                                CategoryProductId = product.Id,                                
                                ActionTaken = 0, // Restocking
                                OldQuantity = product.Units,
                                NewQuantity = product.Units + Units,
                                QuantityAction = +Units,
                                PurchasePrice = PurchasePrice,
                                SalePrice = CustomerPurchasePrice,
                                Remarks = $"Restocked {Units} units of {product.ProductName} at a purchase price {PurchasePrice:C} each.",
                                ReferenceId = $"REF-NEWSTOCK-{product.Id}-{DateTime.Now:yyyyMMddHHmmss}",
                                UserAccountId = (int)defaultUserAccount.OnlineUserAccountId,
                                CreatedAt = DateTime.Now
                            };
                            await dataObjects.TrackInventoryChange(inventoryTracker);

                            UnSyncedObject unSyncedObject = new UnSyncedObject
                            {
                                onlineUnSyncedObjectId = product.OnlineCategoryProductId,
                                objectId = StockItem.Id,
                                objectTableName = "CategoryProducts",
                                actionTaken = 2,
                                fromFullJid = defaultUserAccount.FullJid,
                                toFullJid = defaultUserAccount.BareJid,
                                OnlineStoreId = currentStore.OnlineStoreId,
                                AuthToken = defaultUserAccount.AuthToken,
                                AccountType = accountType

                            };
                            var savedUnSyncedObject = await dataObjects.UnSyncedObjects.SaveAndReturnEntityAsync(unSyncedObject);
                            if (savedUnSyncedObject != null)
                            {
                                using var pipe = new NamedPipeClientStream(
                                    ".",
                                    "ChoziShopUnSyncedObjectsPipe",
                                    PipeDirection.Out);
                                try
                                {
                                    await pipe.ConnectAsync(5000);
                                    var json = JsonConvert.SerializeObject(savedUnSyncedObject);
                                    var bytes = Encoding.UTF8.GetBytes(json);
                                    await pipe.WriteAsync(bytes, 0, bytes.Length);
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine("pipe connection failed "+ex.Message);
                                }
                            }
                            RestockCompleteHandler?.Invoke(this, true);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Missing required fields. Please ensure you have entered the purchase price, units, and customer purchase price.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LoadInventoryLog(StockItem stockItem)
        {
            InventoryTrackers = await dataObjects.FindProductInventoryLogHistory(stockItem.Id);
            foreach(var inventoryTracker in InventoryTrackers)
            {
                inventoryTracker.ActionTakenByUserName = _userLookupCache.TryGetValue(inventoryTracker.UserAccountId, out var userName)
                    ? userName : "Unknown";
                Debug.WriteLine($"User Account Id: {inventoryTracker.UserAccountId}");
            }
            UpdateItemNumbers();
        }
       
        private void UpdateItemNumbers()
        {
            for (int i = 0; i < InventoryTrackers.Count; i++)
            {                
                InventoryTrackers[i].ItemNo = i + 1;
            }
        }

        async Task LoadUserAccounts()
        {
            _userLookupCache.Clear();
            var keepers = await dataObjects.GetAllKeepersAsync();
            foreach (var keeper in keepers)
            {
                _userLookupCache.TryAdd(keeper.OnlineKeeperId, keeper.FullName);
            }

            var merchant = await dataObjects.GetMerchantAsync();
            _userLookupCache.TryAdd(merchant.OnlineMerchantId, merchant.FullName);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
