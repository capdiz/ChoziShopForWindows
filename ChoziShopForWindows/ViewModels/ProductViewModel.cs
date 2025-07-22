using ChoziShop.Data.Models;
using ChoziShopForWindows.Data;
using ChoziShopForWindows.models;
using ChoziShopForWindows.Services;
using HandyControl.Controls;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
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
using MessageBox = System.Windows.MessageBox;


namespace ChoziShopForWindows.ViewModels
{
    public class ProductViewModel : INotifyPropertyChanged
    {
        private readonly IDataObjects dataObjects;
        private readonly IAuthTokenProvider authTokenProvider;

        private readonly string _originalProductName;
        private readonly decimal _originalUnitCost;
        private readonly int _originalUnits;
        private readonly string _originalMeasurement;
        private readonly string _originalValueMetric;
        private readonly string _originalRemarks;

        private int _productId;
        private CategoryProduct categoryProduct;
        private string productName;
        private decimal unitCost;
        private int units;
        private string remarks;
        private string measurement;
        private string valueMetric;
        private DefaultUserAccount _defaultUserAccount;

       
        public EventHandler<bool> ProductUpdateCompleteHandler;
        public EventHandler<CategoryProduct> ProductEditedHandler;

        public ProductViewModel(IDataObjects dataObjects, IServiceProvider serviceProvider, int productId)
        {
            this.dataObjects = dataObjects;
            this.authTokenProvider = serviceProvider.GetRequiredService<IAuthTokenProvider>();
            ProductId = productId;
            LoadCategoryProduct();
            _originalProductName = CategoryProduct.ProductName;
            _originalUnitCost = CategoryProduct.UnitCost;
            _originalUnits = CategoryProduct.Units;
            _originalMeasurement = CategoryProduct.Measurement;
            _originalValueMetric = CategoryProduct.ValueMetric;
            _originalRemarks = CategoryProduct.Remarks;

            productName = _originalProductName;
            unitCost = _originalUnitCost;
            units = _originalUnits;
            measurement = _originalMeasurement;
            valueMetric = _originalValueMetric;
            remarks = _originalRemarks;
            _defaultUserAccount = ApplicationState.Instance.DefaultAccount;
            UpdateProductCommand = new Commands.RelayCommand(async () => await UpdateProductDetails());
            
        }

        public ProductViewModel(IDataObjects dataObjects, IServiceProvider serviceProvider, CategoryProduct categoryProduct)
        {
            this.dataObjects = dataObjects;
            this.authTokenProvider = serviceProvider.GetRequiredService<IAuthTokenProvider>();
            CategoryProduct = categoryProduct ?? throw new ArgumentNullException(nameof(categoryProduct));
            _originalProductName = categoryProduct.ProductName;
            _originalUnitCost = categoryProduct.UnitCost;
            _originalUnits = categoryProduct.Units;
            _originalMeasurement = categoryProduct.Measurement;
            _originalValueMetric = categoryProduct.ValueMetric;
            _originalRemarks = categoryProduct.Remarks;

            ProductId = categoryProduct.Id;
            _defaultUserAccount = ApplicationState.Instance.DefaultAccount;
            productName = _originalProductName;
            unitCost = _originalUnitCost;
            units = _originalUnits; 
            measurement = _originalMeasurement;
            valueMetric = _originalValueMetric;
            remarks = _originalRemarks;
            UpdateProductCommand = new Commands.RelayCommand(async () => await UpdateProductDetails());
        }

        public int ProductId
        {
            get => _productId;
            set
            {
                if (_productId != value)
                {
                    _productId = value;
                    OnPropertyChanged();
                    
                }
            }
        }

        public CategoryProduct CategoryProduct
        {
            get => categoryProduct;
            private set
            {
                if (categoryProduct != value)
                {
                    categoryProduct = value;
                    OnPropertyChanged();
                }
            }
        }

        public string PreviousProductName
        {
            get
            {
                return _originalProductName;
            }
        }

        public decimal PreviousUnitCost
        {
            get
            {
                return _originalUnitCost;
            }
        }

        public int PreviousUnits
        {
            get
            {
                return _originalUnits;
            }
        }

        public string PreviousMeasurement
        {
            get
            {
                return _originalMeasurement;
            }
        }

        public string PreviousValueMetric
        {
            get
            {
                return _originalValueMetric;
            }
        }

        public string PreviousRemarks
        {
            get
            {
                return _originalRemarks;
            }
        }

        public string ProductName
        {
            get { return productName; }
            set
            {
                if (productName != value)
                {
                    Debug.WriteLine($"Setting ProductName to: {value}");
                    productName = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal UnitCost
        {
            get { return unitCost; }
            set
            {
                if (unitCost != value)
                {
                    unitCost = value;
                    OnPropertyChanged();
                }
            }
        }

        public int Units
        {
            get { return units; }
            set
            {
                if (units != value)
                {
                    units = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Measurement
        {
            get { return measurement; }
            set
            {
                if (measurement != value)
                {
                    measurement = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ValueMetric
        {
            get { return valueMetric; }
            set
            {
                if (valueMetric != value)
                {
                    valueMetric = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Remarks
        {
            get { return remarks; }
            set
            {
                if (remarks != value)
                {
                    remarks = value;
                    OnPropertyChanged();
                }
            }
        }

        public string EditProductHeader
        {
            get
            {
                return $"Edit more {CategoryProduct.ProductName} Details.";
            }
        }

        public ICommand UpdateProductCommand { get; }

        private async Task UpdateProductDetails()
        {
            if (CategoryProduct == null)
            {
                throw new InvalidOperationException("CategoryProduct is not loaded.");
            }

            if (!string.IsNullOrEmpty(PreviousProductName) && string.IsNullOrEmpty(ProductName))
            {
                ProductName = PreviousProductName;
            }

            if (!string.IsNullOrEmpty(PreviousUnitCost.ToString()) && UnitCost <= 0)
            {
                UnitCost = PreviousUnitCost;
            }

            if (Units <= 0 && PreviousUnits > 0)
            {
                Units = PreviousUnits;
            }

            if (!string.IsNullOrEmpty(PreviousMeasurement) && string.IsNullOrEmpty(Measurement))
            {
                Measurement = PreviousMeasurement;
            }

            if (!string.IsNullOrEmpty(PreviousValueMetric) && string.IsNullOrEmpty(ValueMetric))
            {
                ValueMetric = PreviousValueMetric;
            }

            if (!string.IsNullOrEmpty(PreviousRemarks) && string.IsNullOrEmpty(Remarks))
            {
                Remarks = PreviousRemarks;
            }

            Debug.WriteLine($"Remarks: {Remarks} vs Previous Remarks: {PreviousRemarks} vs remarks: {remarks}");
            bool hasChanges =
                ProductName != PreviousProductName ||
                UnitCost != PreviousUnitCost ||
                Units != PreviousUnits ||
                Measurement != PreviousMeasurement ||
                ValueMetric != PreviousValueMetric ||
                Remarks != PreviousRemarks;

            if (!hasChanges)
            {
                
                ProductUpdateCompleteHandler?.Invoke(this, false);               
                return;
            }


            // Update the properties of CategoryProduct
            CategoryProduct.ProductName = ProductName;
            CategoryProduct.UnitCost = UnitCost;
            CategoryProduct.Units = Units;
            CategoryProduct.Measurement = Measurement;
            CategoryProduct.ValueMetric = ValueMetric;
            CategoryProduct.Remarks = Remarks;



            if (ProductName == PreviousProductName &&
                UnitCost == PreviousUnitCost &&
                Units == PreviousUnits &&
               Measurement == PreviousMeasurement &&
               ValueMetric == PreviousValueMetric &&
               Remarks == PreviousRemarks)
            {
                ProductUpdateCompleteHandler?.Invoke(this, false);
                return;
            }

            // Save the updated product
            var updatedProduct = await dataObjects.CategoryProducts.UpdateAndReturnEntityAsync(categoryProduct);
            if (updatedProduct != null)
            {
                var defaultUserAccount = ApplicationState.Instance.DefaultAccount;
                var currentMerchant = authTokenProvider.GetCurrentMerchantAccount();
                var currentStore = ApplicationState.Instance.CurrentStore;
                if (defaultUserAccount != null && currentStore != null)
                {
                    string accountType = defaultUserAccount.AccountType == 1 ? "merchant" : "keeper";
                    UnSyncedObject unSyncedObject = new UnSyncedObject
                    {
                        OnlineStoreId = currentStore.OnlineStoreId,
                        onlineUnSyncedObjectId = updatedProduct.OnlineCategoryProductId,
                        objectId = updatedProduct.Id,
                        objectTableName = "CategoryProducts",
                        actionTaken = 2,
                        fromFullJid = defaultUserAccount.FullJid,
                        toFullJid = currentMerchant.FullJid,
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
                            Debug.WriteLine("connection to pipe failed: ", ex.Message);
                        }
                    }


                    // Fetch the last saved InventoryTracker for product if it doesn't exist, we create one
                    var lastInventoryLog = await dataObjects.GetLastInventoryTrackerForProduct(ProductId);
                    if (lastInventoryLog != null)
                    {

                        // Use some prior information like units, quantity, e.t.c for new log entry
                        var inventoryTracker = new InventoryTracker
                        {
                            CategoryProductId = CategoryProduct.Id,
                            InventoryId = lastInventoryLog.InventoryId, // Assuming you want to use the same inventory
                            ActionTaken = 2, // Update action
                            OldQuantity = lastInventoryLog.NewQuantity,
                            NewQuantity = lastInventoryLog.NewQuantity, // Assuming no change in quantity
                            UserAccountId = (int)_defaultUserAccount.OnlineUserAccountId,
                            QuantityAction = lastInventoryLog.QuantityAction,
                            ReferenceId = $"REF-{CategoryProduct.Id}-{DateTime.Now:yyyyMMddHHmmss}",
                            SalePrice = 0,
                            Remarks = $"Updated product details. Previous Values: [Name: {PreviousProductName}] - [Measurement: {PreviousMeasurement}] - [Metric: ${PreviousValueMetric} ",
                            CreatedAt = DateTime.Now
                        };
                        await dataObjects.TrackInventoryChange(inventoryTracker);
                        MessageBox.Show($"Product {CategoryProduct.ProductName} details updated successfully.", "Success");

                    }
                    else
                    {
                        var inventoryTracker = new InventoryTracker
                        {
                            CategoryProductId = CategoryProduct.Id,
                            InventoryId = 0, // Assuming no inventory is associated yet
                            ActionTaken = 2, // Update action
                            OldQuantity = CategoryProduct.Units,
                            NewQuantity = CategoryProduct.Units,
                            QuantityAction = 0,
                            UserAccountId = 14,
                            ReferenceId = $"REF-{CategoryProduct.Id}-{DateTime.Now:yyyyMMddHHmmss}",
                            SalePrice = 0,
                            Remarks = $"Updated product details. Previous Values: [Name: {PreviousProductName}] - [Measurement: {PreviousMeasurement}] - [Metric: ${PreviousValueMetric} ",
                            CreatedAt = DateTime.Now
                        };
                        await dataObjects.TrackInventoryChange(inventoryTracker);
                        MessageBox.Show($"Product {CategoryProduct.ProductName} details updated successfully.", "Success");
                    }
                }
                ProductUpdateCompleteHandler?.Invoke(this, true);
                ProductEditedHandler?.Invoke(this, updatedProduct);
            }
        }

        private async void LoadCategoryProduct()
        {
            // Assuming you have a method to fetch the product by ID
            CategoryProduct = await dataObjects.GetCategoryProductByIdAsync(ProductId);
            if (CategoryProduct == null)
            {
                throw new Exception($"Product with ID {ProductId} not found.");
            }
          

            
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
