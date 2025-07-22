using ChoziShop.Data.Models;
using ChoziShopForWindows.Data;
using ChoziShopForWindows.MerchantsApi;
using ChoziShopForWindows.models;
using ChoziShopForWindows.Serialized;
using ChoziShopForWindows.Services;
using ChoziShopForWindows.Validations;
using ChoziShopForWindows.Views;
using ChoziShopSharedConnectivity.Shared;
using HandyControl.Controls;
using HandyControl.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.DirectoryServices;
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
    public class InventoryViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private readonly IDataObjects _dataObjects;
        private readonly ITransactionService _transactionService;
        private readonly IAuthTokenProvider _authTokenProvider;
        private readonly InternetConnectivityMonitorService _internetConnectivityMonitorService;
        private readonly Dictionary<string, List<string>> _errors = new();

        private BaseApi _baseApi;
        private IServiceProvider _services;

        private int _loggedInUserAccountType;

        private string _productName;
        private string _remarks;
        private decimal _unitCost;
        private int _units;
        private string _measurement;
        private string _valueMetric;    

        private string _categorySectionsCountHeader;
        private string _selectedCategorySectionName;
        private string _newProductHeader;
        private string _productListHeader;
        private string _editItemHeader;

        private ObservableCollection<CategorySection> _categorySections=new();
        private ObservableCollection<CategoryProduct> _productList=new();
        private ObservableCollection<StoreCategory> _defaultStoreCategories=new();

        private ObservableCollection<StoreCategory> _selectedStoreCategories;

        private CategorySection _selectedCategorySection;
        private CategoryProduct _selectedCategoryProduct;
        private StoreCategory _selectedStoreCategory;

        private List<string> _defaultCategorySectionProductNames;
        private ObservableCollection<string> _productNames = new();

        private string _addProductBtnText;
        private string _deleteItemHeader;
        private string _addMultipleCategorySectionsHeader;        
        
        private bool _isAddProductBtnVisible = false;
        private bool _isViewProductListBtnVisible=false;
        private bool _isCategoryProductSelected;
        private bool _isInternetConnected;
        private bool _isAddMultipleCategorySectionsBtnVisible = false;
        private bool _isAddCategorySectionBtnEnabled;
        private bool _isPopupOpen;

        private bool _isDisableAllButtonControls = false;

        private int _selectedIndex;
        
        private ConnectivityStatus _connectivityStatus;
        private NewCategorySectionDialog newCategorySectionDialog;
        private EditProductDialog editProductDialog;
        private NewProductDialog newProductDialog;
        private ProductListDialog productListDialog;

        public InventoryViewModel(IDataObjects dataObjects, IServiceProvider services)
        {
            _dataObjects = dataObjects;
            _services = services;
            _transactionService = _services.GetRequiredService<ITransactionService>();
            _authTokenProvider = _services.GetRequiredService<IAuthTokenProvider>();
            _transactionService.SetAuthTokenProvider(_authTokenProvider);
            _internetConnectivityMonitorService = _services.GetRequiredService<InternetConnectivityMonitorService>();
            // monitor internet connectivity and sync any new category section that might have been added without a connection
            _internetConnectivityMonitorService.StatusChanged += OnInternetStatusChanged;


            _ = LoadCategorySectionSales();
            IsAddCategorySectionBtnEnabled = true;
            IsDisableAllButtonControls = true;
            IsAddProductBtnVisible = false;
            IsViewProductListBtnVisible = false;
            IsCategoryProductSelected = false;
            IsAddMultipleCategorySectionsBtnVisible = false;

            AddNewProductCommand = new Commands.RelayCommand(_ => ShowNewProductDialog());
            ViewProductListCommand = new Commands.RelayCommand(_ => ShowProductListDialog());
            AddNewCategoryCommand = new Commands.RelayCommand(_ => ShowNewCategoryDialog());
            AddSelectedCategorySectionsToInventoryCommand = new Commands.RelayCommand(_ => AddSelectedCategorySectionToInventory());
            RemoveCategorySectionCommand = new Commands.RelayCommand(_ => RemoveCategorySectionFromInventory());
            CreateCategoryProductCommand =new Commands.RelayCommand(async _ => await AddCategoryProductToInventory());
            EditProductCommand = new Commands.RelayCommand(_ => ShowEditProductDialog());
            DeleteProductCommand = new Commands.RelayCommand(async _ => await DeleteCategoryProductFromInventory());
        }

        public void SetLoggedInUserAccountType(int accountType)
        {
            _loggedInUserAccountType = accountType;
        }

        public int LoggedInUserAccountType
        {
            get { return _loggedInUserAccountType; }
            set
            {
                _loggedInUserAccountType = value;
                OnPropertyChanged();
            }
        }
        [Required(ErrorMessage = "Product Name is required.")]
        public string ProductName
        {
            get { return _productName; }
            set
            {
                _productName = value;
                ValidateProperty(value);
                OnPropertyChanged();
                FilterProductNames();
            }
        }

        public string Remarks
        {
            get { return _remarks; }
            set
            {
                _remarks = value;                
                OnPropertyChanged();
            }
        }

        [Required(ErrorMessage = "Unit Cost is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Unit Cost must be greater than zero.")]
        public decimal UnitCost
        {
            get { return _unitCost; }
            set
            {
                _unitCost = value;
                ValidateProperty(value);
                OnPropertyChanged();
            }
        }

        [Required(ErrorMessage = "Units is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Units must be at least 1.")]
        public int Units
        {
            get { return _units; }
            set
            {
                _units = value;
                ValidateProperty(value);
                OnPropertyChanged();
            }
        }

        public string Measurement
        {
            get { return _measurement; }
            set
            {
                _measurement = value;
                OnPropertyChanged();
            }
        }

        public string ValueMetric
        {
            get { return _valueMetric; }
            set
            {
                _valueMetric = value;
                OnPropertyChanged();
            }
        }

        public List<string> DefaultyCategorySectionProductNames
        {
            get { return _defaultCategorySectionProductNames; }
            set
            {
                _defaultCategorySectionProductNames = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> ProductNames
        {
            get { return _productNames; }
            set
            {
                _productNames = value;
                OnPropertyChanged();
            }
        }


        public ObservableCollection<CategorySection> CategorySections
        {
            get { return _categorySections; }
            set
            {
                _categorySections = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<CategoryProduct> ProductList
        {
            get { return _productList; }
            set
            {
                _productList = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<StoreCategory> SelectedStoreCategories
        {
            get { return _selectedStoreCategories; }
            set
            {
                if (_selectedStoreCategories != null)
                {
                    _selectedStoreCategories.CollectionChanged -= OnCollectionChanged;
                    UnsubscribeFromItemsChanged(_selectedStoreCategories);
                }
                _selectedStoreCategories = value;
                if (_selectedStoreCategories != null)
                {
                    _selectedStoreCategories.CollectionChanged += OnCollectionChanged;
                    SubscribeToItemsChanged(_selectedStoreCategories);
                }

                OnPropertyChanged();
            }
        }

        public CategorySection SelectedCategorySection
        {
            get { return _selectedCategorySection; }
            set
            {
                _selectedCategorySection = value;
                if (_selectedCategorySection != null)
                {
                    SelectedCategorySectionName = _selectedCategorySection.CategorySectionName;
                    AddProductBtnText = $"Add new Product to {_selectedCategorySection.CategorySectionName}";
                    NewProductHeader = $"Add new Item to {SelectedCategorySectionName}";
                    ProductListHeader = $"Product Items under {SelectedCategorySectionName} Category";
                    IsAddProductBtnVisible = true;
                    if (_selectedCategorySection.NoOfProducts > 0)
                    {
                        ProductList.Clear();
                        foreach (var item in _selectedCategorySection.CategoryProducts)
                        {
                            if (item.IsDeleted) continue; // Skip deleted items
                            ProductList.Add(item);
                        }
                        IsViewProductListBtnVisible = true;
                    }
                    else
                    {
                        ProductList.Clear();
                        IsViewProductListBtnVisible = false;
                    }

                    if (SelectedCategorySection.DefaultCategorySectionProducts.Count > 0)
                    {
                        DefaultyCategorySectionProductNames = SelectedCategorySection.DefaultCategorySectionProducts
                            .Select(p => p.ProductName).Distinct().ToList();
                    }
                    else
                    {
                        DefaultyCategorySectionProductNames = new List<string>();
                    }
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<StoreCategory> DefaultCategories
        {
            get { return _defaultStoreCategories; }
            set
            {
                _defaultStoreCategories = value;
                OnPropertyChanged();
            }
        }

        public CategoryProduct SelectedCategoryProduct
        {
            get { return _selectedCategoryProduct; }
            set
            {

                _selectedCategoryProduct = value;
                if (_selectedCategoryProduct != null)
                {
                    IsCategoryProductSelected = true;
                    DeleteItemHeader = $"Remove {_selectedCategoryProduct.ProductName} from Inventory";
                    EditItemHeader = $"Edit {_selectedCategoryProduct.ProductName} Details";
                    if (_selectedCategoryProduct.OnlineCategorySectionId == 0)
                    {
                        if(_selectedCategorySection != null)
                        {
                            _selectedCategoryProduct.OnlineCategorySectionId = _selectedCategorySection.OnlineCategorySectionId;
                        }
                    }
                }
                OnPropertyChanged();

            }
        }

        public StoreCategory SelectedStoreCategory
        {
            get { return _selectedStoreCategory; }
            set
            {
                _selectedStoreCategory = value;
                if (SelectedStoreCategories == null)
                {
                    SelectedStoreCategories = new ObservableCollection<StoreCategory>();
                }
                SelectedStoreCategories.Add(_selectedStoreCategory);
                OnPropertyChanged();
            }
        }

        public string AddProductBtnText
        {
            get { return _addProductBtnText; }
            set
            {
                _addProductBtnText = value;
                OnPropertyChanged();
            }
        }

        public string EditItemHeader
        {
            get { return _editItemHeader; }
            set
            {
                _editItemHeader = value;
                OnPropertyChanged();
            }
        }

        public string DeleteItemHeader
        {
            get { return _deleteItemHeader; }
            set
            {
                _deleteItemHeader = value;
                OnPropertyChanged();
            }
        }

        public string AddMultipleCategorySectionsHeader
        {
            get { return _addMultipleCategorySectionsHeader; }
            set
            {
                _addMultipleCategorySectionsHeader = value;
                OnPropertyChanged();
            }
        }

        public bool IsAddProductBtnVisible
        {
            get { return _isAddProductBtnVisible; }
            set
            {
                _isAddProductBtnVisible = value;
                OnPropertyChanged();
            }
        }

        public bool IsViewProductListBtnVisible
        {
            get { return _isViewProductListBtnVisible; }
            set
            {
                _isViewProductListBtnVisible = value;
                OnPropertyChanged();
            }
        }

        public bool IsCategoryProductSelected
        {
            get { return _isCategoryProductSelected; }
            set
            {
                _isCategoryProductSelected = value;
                OnPropertyChanged();
            }
        }

        public bool IsInternetConnected
        {
            get { return _isInternetConnected; }
            set
            {
                _isInternetConnected = value;
                OnPropertyChanged();
            }
        }

        public bool IsAddMultipleCategorySectionsBtnVisible
        {
            get { return _isAddMultipleCategorySectionsBtnVisible; }
            set
            {
                _isAddMultipleCategorySectionsBtnVisible = value;
                OnPropertyChanged();
            }
        }

        public bool IsAddCategorySectionBtnEnabled
        {
            get { return _isAddCategorySectionBtnEnabled; }
            set
            {
                _isAddCategorySectionBtnEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool IsDisableAllButtonControls
        {
            get { return _isDisableAllButtonControls; }
            set
            {
                _isDisableAllButtonControls = value;
                OnPropertyChanged();
            }
        }

        public bool IsPopupOpen
        {
            get { return _isPopupOpen; }
            set
            {
                if(_isPopupOpen==value) return; // No change, no need to notify
                _isPopupOpen = value;
                OnPropertyChanged();

                if (value)
                {
                    Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                      var control = Application.Current.Windows.OfType<NewProductDialog>().FirstOrDefault();
                        (control as NewProductDialog)?.FocusListBox();
                    });
                }
            }
        }

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                _selectedIndex = value;
                OnPropertyChanged();
            }
        }

        public string CategorySectionsCountHeader
        {
            get { return _categorySectionsCountHeader; }
            set
            {
                _categorySectionsCountHeader = value;
                OnPropertyChanged();
            }
        }

        public string SelectedCategorySectionName
        {
            get { return _selectedCategorySectionName; }
            set
            {
                _selectedCategorySectionName = value;
                OnPropertyChanged();
            }
        }

        public string NewProductHeader
        {
            get { return _newProductHeader; }
            set
            {
                _newProductHeader = value;
                OnPropertyChanged();
            }
        }

        public string ProductListHeader
        {
            get { return _productListHeader; }
            set
            {
                _productListHeader = value;
                OnPropertyChanged();
            }
        }

        public ICommand ViewProductListCommand { get; }
        public ICommand AddNewProductCommand { get; }
        public ICommand AddNewCategoryCommand { get; }
        public ICommand AddSelectedCategorySectionsToInventoryCommand { get; }
        public ICommand RemoveCategorySectionCommand { get; }
        public ICommand CreateCategoryProductCommand { get; }

        public ICommand EditProductCommand { get; }
        public ICommand DeleteProductCommand { get; }

        public void SelectNext()
        {
            if(ProductNames.Count > 0 && SelectedIndex < ProductNames.Count-1)
            {
                SelectedIndex++;
            }
        }

        public void SelectPrevious()
        {
            if (ProductNames.Count > 0 && SelectedIndex > 0)
            {
                SelectedIndex--;
            }
        }

        public void ApplySuggestion()
        {
            if (SelectedIndex >= 0 && SelectedIndex < ProductNames.Count)
            {
                ProductName = ProductNames[SelectedIndex];
                IsPopupOpen = false;
                SelectedIndex = -1; // Reset index after applying suggestion
            }
        }

        async Task LoadCategorySectionSales()
        {
            var currentStore = ApplicationState.Instance.CurrentStore;
            if (currentStore != null)
            {
                var orders = await _dataObjects.GetStoreOrdersAsync(currentStore.InventoryId);
                var productsSold = orders.SelectMany(o => o.OrderProductItems).ToList();
                Debug.WriteLine($"Products sold: {productsSold.Count}");

                var categorySections = await _dataObjects.GetStoreCategorySectionsAsync(currentStore.InventoryId);
                _categorySections.Clear();
                foreach (var categorySection in categorySections)
                {
                    categorySection.CategoryProducts = categorySection.CategoryProducts.Where(cp => !cp.IsDeleted).ToList();
                    categorySection.NoOfProducts = categorySection.CategoryProducts.Count();
                    int categorySalesCount = productsSold.Where(p => p.CategorySectionId == categorySection.Id).Count();

                    categorySection.SalesAmount =
                        productsSold.Where(p => p.CategorySectionId == categorySection.Id)
                        .Sum(p => p.UnitCost * categorySalesCount);
                    categorySection.LowOnStockItemsCount =
                        _dataObjects.GroupStockItemsByStatusAsync((List<CategoryProduct>)categorySection.CategoryProducts, Enums.ProductInventoryStatus.Low).Count();
                    categorySection.OutOfStockItemsCount =
                        _dataObjects.GroupStockItemsByStatusAsync((List<CategoryProduct>)categorySection.CategoryProducts, Enums.ProductInventoryStatus.Empty).Count();
                    categorySection.CriticalOnStockItemsCount =
                        _dataObjects.GroupStockItemsByStatusAsync((List<CategoryProduct>)categorySection.CategoryProducts, Enums.ProductInventoryStatus.Critical).Count();
                    Debug.WriteLine($"Category Section: {categorySection.CategorySectionName} - Amount: {categorySection.SalesAmount}");
                    await GetCategorySectionDefaultProducts(categorySection);
                    _categorySections.Add(categorySection);

                }
                CategorySectionsCountHeader = $"{_categorySections.Count} Category Sections found in your inventory";
            }
        }

        private async Task GetCategorySectionDefaultProducts(CategorySection categorySection)
        {
            _baseApi = new BaseApi(ApplicationState.Instance.DefaultAccount.AuthToken);
            var defaultCategorySectionProducts = await _dataObjects.GetCategorySectionDefaultProductsByCategorySectionIdAsync(categorySection.Id);
            if (defaultCategorySectionProducts.Count==0)
            {
                long onlineCategoryId = await _dataObjects.GetOnlineCategoryIdByName(categorySection.CategorySectionName);
                if (onlineCategoryId > 0)
                {
                    var defaultCategoryProductsResponse =
                        await _baseApi.GetDefaultCategorySectionProductsAsync(
                            onlineCategoryId,
                            ApplicationState.Instance.DefaultAccount.DefaultAccountType);
                    if (defaultCategoryProductsResponse != null && defaultCategoryProductsResponse.DefaultCategoryProducts.Count > 0)
                    {
                        foreach (var defaultCategoryProduct in defaultCategoryProductsResponse.DefaultCategoryProducts)
                        {
                            if (!string.IsNullOrEmpty(defaultCategoryProduct.ProductName))
                            {
                                DefaultCategorySectionProduct defaultCategorySectionProduct = new DefaultCategorySectionProduct
                                {
                                    CategorySectionId = (int)categorySection.OnlineCategorySectionId,
                                    CategorySection = categorySection,
                                    ProductName = defaultCategoryProduct.ProductName,

                                };
                                var savedDefaultCategorySectionProduct =
                                    await _dataObjects.DefaultCategorySectionProducts.SaveAndReturnEntityAsync(defaultCategorySectionProduct);
                                if (savedDefaultCategorySectionProduct != null)
                                {
                                    categorySection.DefaultCategorySectionProducts.Add(savedDefaultCategorySectionProduct);
                                    Debug.WriteLine($"Saved Default Category Section Product: {savedDefaultCategorySectionProduct.ProductName}");
                                }
                            }
                        }
                    }
                }
                else
                {
                    Debug.WriteLine($"No online category id found for {categorySection.CategorySectionName}");
                }
            }
            else
            {
                categorySection.DefaultCategorySectionProducts = 
                    new ObservableCollection<DefaultCategorySectionProduct>(defaultCategorySectionProducts);
            }
        }

        private async Task AddCategoryProductToInventory()
        {
            if (SelectedCategorySection != null)
            {
                var currentStore = ApplicationState.Instance.CurrentStore;
                var currentMerchant = _authTokenProvider.GetCurrentMerchantAccount();
                if (currentMerchant != null && currentStore != null)
                {
                    CategoryProduct categoryProduct = new CategoryProduct
                    {
                        CategorySection = SelectedCategorySection,
                        OnlineCategorySectionId = SelectedCategorySection.OnlineCategorySectionId,
                        ProductName = ProductName,
                        Remarks = Remarks,
                        UnitCost = UnitCost,
                        Units = Units,
                        Measurement = Measurement,
                        ValueMetric = ValueMetric,
                        Currency = "UGX"
                    };
                    var savedCategoryProduct = await _dataObjects.CategoryProducts.SaveAndReturnEntityAsync(categoryProduct);
                    if (savedCategoryProduct != null)
                    {
                        CategorySection categorySection = SelectedCategorySection;
                        categorySection.NoOfProducts++;
                        categorySection.CategoryProducts.Add(savedCategoryProduct);
                        SelectedCategorySection = categorySection;

                        ProductList.Add(savedCategoryProduct);
                        IsDisableAllButtonControls = false;
                        

                        // Create an UnSyncedObject for the new product
                        var unSyncedObject = new UnSyncedObject
                        {
                            OnlineStoreId = ApplicationState.Instance.CurrentStore.OnlineStoreId,
                            onlineUnSyncedObjectId = savedCategoryProduct.OnlineCategoryProductId,
                            objectId = savedCategoryProduct.Id,
                            objectTableName = "CategoryProducts",
                            syncStatus = 0, // object change has been synced to api
                            actionTaken = 1, // category product object has been created
                            fromFullJid = ApplicationState.Instance.DefaultAccount.FullJid,
                            toFullJid = currentMerchant.FullJid,
                            AuthToken = ApplicationState.Instance.DefaultAccount.AuthToken,
                            AccountType = ApplicationState.Instance.DefaultAccount.DefaultAccountType
                        };

                        var savedUnSyncedObject = await _dataObjects.UnSyncedObjects.SaveAndReturnEntityAsync(unSyncedObject);
                        if (IsInternetConnected)
                        {
                            if (savedUnSyncedObject != null)
                            {
                                using var pipe = new NamedPipeClientStream(
                                    ".",
                                    "ChoziShopUnSyncedObjectsPipe",
                                    PipeDirection.Out
                                    );
                                try
                                {
                                    await pipe.ConnectAsync(5000);
                                    var json = JsonConvert.SerializeObject(savedUnSyncedObject);
                                    var bytes = Encoding.UTF8.GetBytes(json);
                                    await pipe.WriteAsync(bytes, 0, bytes.Length);
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine("Connection to pipe failed: ", ex.Message);
                                }
                            }
                        }
                        // Fetch the last saved InventoryTracker for product if it doesn't exist, we create one
                        var lastInventoryLog = await _dataObjects.GetLastInventoryTrackerForProduct(savedCategoryProduct.Id);
                        if (lastInventoryLog != null)
                        {

                            // Use some prior information like units, quantity, e.t.c for new log entry
                            var inventoryTracker = new InventoryTracker
                            {
                                CategoryProductId = savedCategoryProduct.Id,
                                InventoryId = lastInventoryLog.InventoryId, // Assuming you want to use the same inventory
                                ActionTaken = 2, // Update action
                                OldQuantity = lastInventoryLog.NewQuantity,
                                NewQuantity = lastInventoryLog.NewQuantity, // Assuming no change in quantity
                                UserAccountId = (int)ApplicationState.Instance.DefaultAccount.OnlineUserAccountId,
                                QuantityAction = lastInventoryLog.QuantityAction,
                                ReferenceId = $"REF-{savedCategoryProduct.Id}-{DateTime.Now:yyyyMMddHHmmss}",
                                SalePrice = 0,
                                Remarks = $"Added new product. Name: [{savedCategoryProduct.ProductName}] - [Unit Cost: {savedCategoryProduct.UnitCost}] ",
                                CreatedAt = DateTime.Now
                            };
                            await _dataObjects.TrackInventoryChange(inventoryTracker);
                            newProductDialog?.close();
                            MessageBox.Show($"{savedCategoryProduct.ProductName} has been added to {SelectedCategorySection.CategorySectionName} successfully.",
                            "Product Added", MessageBoxButton.OK, MessageBoxImage.Information);

                        }
                        else
                        {
                            var inventoryTracker = new InventoryTracker
                            {
                                CategoryProductId = savedCategoryProduct.Id,
                                InventoryId = currentStore.OnlineStoreId, // Assuming no inventory is associated yet
                                ActionTaken = 2, // Update action
                                OldQuantity = savedCategoryProduct.Units,
                                NewQuantity = savedCategoryProduct.Units,
                                QuantityAction = 0,
                                UserAccountId = (int)ApplicationState.Instance.DefaultAccount.OnlineUserAccountId,
                                ReferenceId = $"REF-{savedCategoryProduct.Id}-{DateTime.Now:yyyyMMddHHmmss}",
                                SalePrice = 0,
                                Remarks = $"Added new product details. [Name: {savedCategoryProduct.ProductName}] - [Unit Cost: {savedCategoryProduct.UnitCost}]",
                                CreatedAt = DateTime.Now
                            };
                            await _dataObjects.TrackInventoryChange(inventoryTracker);
                            newProductDialog?.close();
                            IsDisableAllButtonControls = false;
                            MessageBox.Show($"{savedCategoryProduct.ProductName} has been added to {SelectedCategorySection.CategorySectionName} successfully.",
                            "Product Added", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Failed to add new Product. Please try again later.", "Error Adding Product", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                }
            }
            else
            {
                MessageBox.Show("Please select a Category Section to add a new Product.", "No Category Section Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ShowEditProductDialog()
        {
            if (SelectedCategoryProduct != null)
            {
                ProductViewModel productViewModel = new ProductViewModel(_dataObjects, _services, SelectedCategoryProduct);
                Debug.WriteLine($"Category Section Id: {SelectedCategoryProduct.OnlineCategorySectionId}");
                productViewModel.ProductEditedHandler += OnProductEdited;
                productViewModel.ProductUpdateCompleteHandler += OnProductNotChanged;
                editProductDialog = new EditProductDialog(productViewModel);
                Dialog.Show(editProductDialog);
            }
            else
            {
                MessageBox.Show("Please select a Product to edit.", "No Product Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OnProductNotChanged(object sender, bool isEdited)
        {
            if (!isEdited)
            {
                editProductDialog.close();
                MessageBox.Show("No changes were made to the Product.", "No Changes", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void OnProductEdited(object sender, CategoryProduct editedProduct)
        {
            if (editedProduct != null)
            {
                var existingProduct = ProductList.FirstOrDefault(p => p.Id == editedProduct.Id);
                if (existingProduct != null)
                {
                    
                    
                    existingProduct.ProductName = editedProduct.ProductName;
                    existingProduct.UnitCost = editedProduct.UnitCost;
                    existingProduct.Units = editedProduct.Units;
                    existingProduct.Measurement = editedProduct.Measurement;
                    existingProduct.ValueMetric = editedProduct.ValueMetric;
                    existingProduct.Remarks = editedProduct.Remarks;
                    
                }
            }
            editProductDialog?.close();
        }

        private async Task DeleteCategoryProductFromInventory()
        {
            Debug.WriteLine($"Deleting product: {SelectedCategoryProduct?.ProductName} from inventory.");
            if (SelectedCategoryProduct != null)
            {                
                var categoryProductId = SelectedCategoryProduct.Id;
                string deletedProductName = SelectedCategoryProduct.ProductName;
                var currentStore = ApplicationState.Instance.CurrentStore;
                var currentMerchant = _authTokenProvider.GetCurrentMerchantAccount();
                if (currentMerchant != null && currentStore != null)
                {
                    var confirmation = MessageBox.Show($"Are you sure you want to delete {SelectedCategoryProduct.ProductName} from your inventory?",
                        "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (confirmation == MessageBoxResult.Yes)
                    {                                                
                        var isDeleted = await _dataObjects.SoftDeleteCategoryProductAsync(SelectedCategoryProduct.Id);
                        if (isDeleted)
                        {
                            
                            // Create an UnSyncedObject for the deleted product
                            var unSyncedObject = new UnSyncedObject
                            {
                                OnlineStoreId = ApplicationState.Instance.CurrentStore.OnlineStoreId,
                                onlineUnSyncedObjectId = SelectedCategoryProduct.OnlineCategoryProductId,
                                objectId = SelectedCategoryProduct.Id,
                                objectTableName = "CategoryProducts",
                                syncStatus = 0, // object change has been synced to api
                                actionTaken = 3, // category product object has been deleted
                                fromFullJid = ApplicationState.Instance.DefaultAccount.FullJid,
                                toFullJid = currentMerchant.FullJid,
                                AuthToken = ApplicationState.Instance.DefaultAccount.AuthToken,
                                AccountType = ApplicationState.Instance.DefaultAccount.DefaultAccountType
                            };
                            var savedUnSyncedObject = await _dataObjects.UnSyncedObjects.SaveAndReturnEntityAsync(unSyncedObject);
                            if (IsInternetConnected)
                            {
                                if (savedUnSyncedObject != null)
                                {
                                    using var pipe = new NamedPipeClientStream(
                                        ".",
                                        "ChoziShopUnSyncedObjectsPipe",
                                        PipeDirection.Out
                                        );
                                    try
                                    {
                                        await pipe.ConnectAsync(5000);
                                        var json = JsonConvert.SerializeObject(savedUnSyncedObject);
                                        var bytes = Encoding.UTF8.GetBytes(json);
                                        await pipe.WriteAsync(bytes, 0, bytes.Length);
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.WriteLine("Connection to pipe failed: ", ex.Message);
                                    }
                                }
                            }

                            // Fetch the last saved InventoryTracker for product if it doesn't exist, we create one
                            var lastInventoryLog = await _dataObjects.GetLastInventoryTrackerForProduct(categoryProductId);

                            if (lastInventoryLog != null)
                            {
                                // Use some prior information like units, quantity, e.t.c for new log entry
                                var inventoryTracker = new InventoryTracker
                                {
                                    CategoryProductId = categoryProductId,
                                    InventoryId = lastInventoryLog.InventoryId, // Assuming you want to use the same inventory
                                    ActionTaken = 4, // item has been discontinued
                                    OldQuantity = lastInventoryLog.NewQuantity,
                                    NewQuantity = lastInventoryLog.NewQuantity, // Assuming no change in quantity
                                    UserAccountId = (int)ApplicationState.Instance.DefaultAccount.OnlineUserAccountId,
                                    QuantityAction = lastInventoryLog.QuantityAction,
                                    ReferenceId = $"REF-{categoryProductId}-{DateTime.Now:yyyyMMddHHmmss}",
                                    SalePrice = 0,
                                    Remarks = $"Deleted product from inventory. Name: [{deletedProductName}] - [Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}] ",
                                    CreatedAt = DateTime.Now
                                };
                                await _dataObjects.TrackInventoryChange(inventoryTracker);
                            }
                            else
                            {
                                var inventoryTracker = new InventoryTracker
                                {
                                    CategoryProductId = categoryProductId,
                                    InventoryId = currentStore.OnlineStoreId, // Assuming no inventory is associated yet
                                    ActionTaken = 4, // Item has been discontinued
                                    OldQuantity = SelectedCategoryProduct.Units,
                                    NewQuantity = SelectedCategoryProduct.Units,
                                    QuantityAction = 0,
                                    UserAccountId = (int)ApplicationState.Instance.DefaultAccount.OnlineUserAccountId,
                                    ReferenceId = $"REF-{categoryProductId}-{DateTime.Now:yyyyMMddHHmmss}",
                                    SalePrice = 0,
                                    Remarks = $"Deleted product from inventory. [Name: {deletedProductName}] - [Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}]",
                                    CreatedAt = DateTime.Now
                                };
                                await _dataObjects.TrackInventoryChange(inventoryTracker);

                            }

                            CategorySection categorySection = SelectedCategorySection;
                            categorySection.NoOfProducts--;
                            categorySection.CategoryProducts.Remove(SelectedCategoryProduct);
                            SelectedCategorySection = categorySection;
                            ProductList.Remove(SelectedCategoryProduct);
                            IsDisableAllButtonControls = true;

                            productListDialog?.close();
                            MessageBox.Show($"{deletedProductName} has been removed from {SelectedCategorySection.CategorySectionName} successfully.",
                            "Product Removed", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                       
                    }
                }
            }
        }

        private async Task AddSelectedCategorySectionToInventory()
        {
            if (IsInternetConnected)
            {
                IsAddCategorySectionBtnEnabled = false;
                List<long> selectedCategoryIds = SelectedStoreCategories.Select(c => c.Id).ToList();
                CategorySectionCall categorySectionCall = new CategorySectionCall();
                categorySectionCall.CategoryIds = selectedCategoryIds;  
                var currentMerchant = _authTokenProvider.GetCurrentMerchantAccount();

                if (_baseApi == null)
                {
                    var currentUserAccount = ApplicationState.Instance.DefaultAccount;
                    _baseApi = new BaseApi(currentUserAccount.AuthToken);
                }
                var store = ApplicationState.Instance.CurrentStore;
                if (store != null)
                {
                    var categorySectionResponse =
                        await _baseApi.CreateCategorySectionAsyncCall(
                            store.OnlineStoreId,
                            ApplicationState.Instance.DefaultAccount.DefaultAccountType,
                            categorySectionCall
                            );
                    if (categorySectionResponse != null && categorySectionResponse.CategorySections != null)
                    {
                        foreach (CategorySectionResponse sectionResponse in categorySectionResponse.CategorySections)
                        {
                            CategorySection categorySection = new CategorySection
                            {
                                OnlineCategorySectionId = sectionResponse.Id,
                                InventoryId= store.InventoryId,
                                CategorySectionName = sectionResponse.CategoryName,                               
                                CreatedAt = sectionResponse.CreatedAt,
                                UpdatedAt = sectionResponse.UpdatedAt,
                            };
                            var savedCategorySection = await _dataObjects.AddCategorySectionAsync(categorySection);
                            if (savedCategorySection != null)
                            {

                                var unsyncedObject = new UnSyncedObject
                                {
                                    OnlineStoreId = ApplicationState.Instance.CurrentStore.OnlineStoreId,
                                    onlineUnSyncedObjectId = savedCategorySection.OnlineCategorySectionId,
                                    objectId = savedCategorySection.Id,
                                    objectTableName = "CategorySections",
                                    syncStatus = 1, // object has been synced to api
                                    actionTaken = 0,
                                    fromFullJid = ApplicationState.Instance.DefaultAccount.FullJid,
                                    toFullJid = currentMerchant.FullJid,
                                    AuthToken = ApplicationState.Instance.DefaultAccount.AuthToken,
                                    AccountType = ApplicationState.Instance.DefaultAccount.DefaultAccountType
                                };
                                var savedUnSyncedObject = await _dataObjects.UnSyncedObjects.SaveAndReturnEntityAsync(unsyncedObject);
                                if (savedUnSyncedObject != null)
                                {
                                    using var pipe = new NamedPipeClientStream(
                                        ".",
                                        "ChoziShopUnSyncedObjectsPipe",
                                        PipeDirection.Out
                                        );
                                    try
                                    {
                                        await pipe.ConnectAsync(5000);
                                        var json = JsonConvert.SerializeObject(savedUnSyncedObject);
                                        var bytes = Encoding.UTF8.GetBytes(json);
                                        await pipe.WriteAsync(bytes, 0, bytes.Length);
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.WriteLine("Connection to pipe failed: ", ex.Message);
                                    }
                                }
                                CategorySections.Add(savedCategorySection);
                                IsDisableAllButtonControls = false;
                            }
                        }
                        IsDisableAllButtonControls = true;
                        newCategorySectionDialog?.close();
                        CategorySectionsCountHeader = $"{CategorySections.Count} Category Sections found in your inventory";
                        MessageBox.Show($"{categorySectionResponse.CategorySections.Count} Category Sections added to your inventory successfully.",
                            "Category Sections Added", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        IsAddCategorySectionBtnEnabled = true;
                        MessageBox.Show("Failed to add new Category Sections to your inventory. Please try again later.",
                            "Error Adding Category Sections", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        
        private async Task SyncDefaultCategories()
        {
            var existingCategorySectionNames = CategorySections.Select(c => c.CategorySectionName).ToList();
            var cachedDefaultCategories = await _dataObjects.GetCachedDefaultCategories(existingCategorySectionNames);
            if (cachedDefaultCategories != null && cachedDefaultCategories.Count() > 0)
            {
                if (DefaultCategories.Count == 0)
                {

                    foreach (var category in cachedDefaultCategories)
                    {
                        if (category != null)
                        {
                            StoreCategory storeCategory = new StoreCategory
                            {
                                Id = category.OnlineCategoryId,
                                CategoryName = category.CategoryName,
                                IconUrl = category.IconUrl,
                                UpdatedAt = category.UpdatedAt                                
                            };

                            DefaultCategories.Add(storeCategory);
                        }
                    }
                }
            }
            else
            {
                if (IsInternetConnected)
                {
                    Debug.WriteLine($"Current user account: {ApplicationState.Instance.DefaultAccount.AuthToken}");
                    BaseApi _baseApi = new BaseApi(ApplicationState.Instance.DefaultAccount.AuthToken);
                    var categoryResponse = await _baseApi.GetDefaultCategoriesAsync(ApplicationState.Instance.DefaultAccount.DefaultAccountType);
                    if (categoryResponse != null && categoryResponse.Categories.Count() > 0)
                    {
                        DefaultCategories.Clear();
                        foreach (var category in categoryResponse.Categories)
                        {
                            if (category != null)
                            {
                                string iconUrl = category.IconUrl ?? "pack://application:,,,/Assets/Images/DefaultCategoryIcon.png";
                                StoreCategory storeCategory = new StoreCategory
                                {
                                    Id = category.OnlineCategoryId,
                                    CategoryName = category.CategoryName,
                                    IconUrl = iconUrl,
                                    UpdatedAt = category.UpdatedAt
                                };
                                Category category1 = new Category
                                {
                                    OnlineCategoryId = category.OnlineCategoryId,
                                    CategoryName = category.CategoryName,
                                    IconUrl = iconUrl,
                                    UpdatedAt = category.UpdatedAt                                  
                                };
                                var savedDefaultCategory = _dataObjects.Categories.SaveAndReturnEntityAsync(category1);
                                if (savedDefaultCategory != null)
                                {
                                    DefaultCategories.Add(storeCategory);
                                }
                            }
                        }
                        Debug.WriteLine($"Default Categories Count: {DefaultCategories.Count}");
                    }
                    else
                    {
                        Debug.WriteLine("No default categories found.");
                    }
                }
                else
                {
                    MessageBox.Show(ProductListHeader + " cannot be accessed without an internet connection. Please check your internet connectivity and try again.",
                        "No Internet Connection", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private async void RemoveCategorySectionFromInventory()
        {
            if (SelectedCategorySection != null)
            {
                IsDisableAllButtonControls = true;
                var confirmation = MessageBox.Show($"Are you sure you want to remove {SelectedCategorySection.CategorySectionName} from your inventory?",
                    "Confirm Removal", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (confirmation == MessageBoxResult.Yes)
                {
                    var currentStore = ApplicationState.Instance.CurrentStore;
                    var currentMerchant = _authTokenProvider.GetCurrentMerchantAccount();
                    if (currentStore != null && currentMerchant != null)
                    {
                        _baseApi = new BaseApi(ApplicationState.Instance.DefaultAccount.AuthToken);
                        var categorySectionResponse = await _baseApi.DeleteCategorySectionAsyncCall(currentStore.OnlineStoreId,
                            SelectedCategorySection.OnlineCategorySectionId, ApplicationState.Instance.DefaultAccount.DefaultAccountType);
                        if (categorySectionResponse!=null && categorySectionResponse.Deleted == 0)
                        {
                            var isDeletedFromDevice = await _dataObjects.CategorySections.DeleteWithStatusAsync(SelectedCategorySection.Id);
                            if (isDeletedFromDevice)
                            {
                                // Create an UnSyncedObject for deleted category section
                                var unSyncedObject = new UnSyncedObject
                                {
                                    OnlineStoreId = ApplicationState.Instance.CurrentStore.OnlineStoreId,
                                    onlineUnSyncedObjectId = SelectedCategorySection.OnlineCategorySectionId,
                                    objectId = SelectedCategorySection.Id,
                                    objectTableName = "CategorySections",
                                    syncStatus = 1, // object change has been synced to api
                                    actionTaken = 4, // object has been deleted
                                    fromFullJid = ApplicationState.Instance.DefaultAccount.FullJid,
                                    toFullJid = currentMerchant.FullJid,
                                    AuthToken = ApplicationState.Instance.DefaultAccount.AuthToken,
                                    AccountType = ApplicationState.Instance.DefaultAccount.DefaultAccountType
                                };
                                var savedUnSyncedObject = await _dataObjects.UnSyncedObjects.SaveAndReturnEntityAsync(unSyncedObject);
                                if (savedUnSyncedObject != null)
                                {
                                    using var pipe = new NamedPipeClientStream(
                                        ".",
                                        "ChoziShopUnSyncedObjectsPipe",
                                        PipeDirection.Out
                                        );
                                    try
                                    {
                                        await pipe.ConnectAsync(5000);
                                        var json = JsonConvert.SerializeObject(savedUnSyncedObject);
                                        var bytes = Encoding.UTF8.GetBytes(json);
                                        await pipe.WriteAsync(bytes, 0, bytes.Length);
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.WriteLine("Connection to pipe failed: ", ex.Message);
                                    }
                                    var deletedCategorySection = SelectedCategorySection;
                                    CategorySections.Remove(SelectedCategorySection);
                                    SelectedCategorySection = null;
                                    MessageBox.Show($"{deletedCategorySection.CategorySectionName} has been removed from your inventory successfully.",
                                        "Category Section Removed", MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Failed to remove the selected Category Section. Please try again later.",
                                "Error Removing Category Section", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a Category Section to remove.", "No Category Section Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void FilterProductNames()
        {
            if (string.IsNullOrEmpty(ProductName))
            {
                IsPopupOpen = false;
                ProductNames.Clear();
                return;
            }
            var filteredNames = DefaultyCategorySectionProductNames
                .Where(name => name.IndexOf(ProductName, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
            ProductNames.Clear();
            foreach (var name in filteredNames)
            {
                ProductNames.Add(name);
            }

            SelectedIndex = ProductNames.Any() ? 0 : -1;
            IsPopupOpen = ProductNames.Any();
        }
       
        private void OnProductListChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(e.NewItems != null)
            {
                SubscribeToCategoryProductsChanged(e.NewItems.Cast<CategoryProduct>());
            }
            if (e.OldItems != null)
            {
                UnsubscribeFromCategoryProductsChanged(e.OldItems.Cast<CategoryProduct>());
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            
            if(e.NewItems != null)
            {
                SubscribeToItemsChanged(e.NewItems.Cast<StoreCategory>());
            }

            if (e.OldItems != null)
            {
                UnsubscribeFromItemsChanged(e.OldItems.Cast<StoreCategory>());
            }
        }

        private void ShowNewProductDialog()
        {
            newProductDialog = _services.GetRequiredService<NewProductDialog>();
            HandyControl.Controls.Dialog.Show(newProductDialog);
        }

        private void ShowProductListDialog()
        {
            productListDialog = _services.GetRequiredService<ProductListDialog>();
            HandyControl.Controls.Dialog.Show(productListDialog);
        }

        private void ShowNewCategoryDialog()
        {
            if (IsInternetConnected)
            {
                newCategorySectionDialog = _services.GetRequiredService<NewCategorySectionDialog>();
                HandyControl.Controls.Dialog.Show(newCategorySectionDialog);
            }
            else
            {
                MessageBox.Show(ProductListHeader + " cannot be accessed without an internet connection. Please check your internet connectivity and try again.",
                    "No Internet Connection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OnInternetStatusChanged(ConnectivityStatus status)
        {
            _connectivityStatus = status;
            IsInternetConnected = _connectivityStatus.IsInternetConnected;
            Debug.WriteLine($"Internet connectivity status changed: {IsInternetConnected}");
            _ = SyncDefaultCategories();
        }

        private void SubscribeToItemsChanged(IEnumerable<StoreCategory> items)
        {
            foreach(var item in items)
            {
                item.PropertyChanged += OnStoreCategoryPropertyChanged;
            }
        }

        private void UnsubscribeFromItemsChanged(IEnumerable<StoreCategory> items)
        {
            foreach (var item in items)
            {
                item.PropertyChanged -= OnStoreCategoryPropertyChanged;
            }
        }

        private void SubscribeToCategoryProductsChanged(IEnumerable<CategoryProduct> products)
        {
            foreach (var product in products)
            {
                product.PropertyChanged += OnProductPropertyChanged;
            }
        }

        private void UnsubscribeFromCategoryProductsChanged(IEnumerable<CategoryProduct> products)
        {
            foreach (var product in products)
            {
                product.PropertyChanged -= OnProductPropertyChanged;
            }
        }

        private void OnProductPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(CategoryProduct.ProductName)
                || e.PropertyName==nameof(CategoryProduct.Remarks)
                || e.PropertyName==nameof(CategoryProduct.UnitCost)
                || e.PropertyName == nameof(CategoryProduct.Units)
                || e.PropertyName == nameof(CategoryProduct.Measurement)
                || e.PropertyName == nameof(CategoryProduct.ValueMetric))
            {
                Debug.WriteLine($"Product Property Changed: {e.PropertyName}");
            }
        }

        private void OnStoreCategoryPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(StoreCategory.IsSelected))
            {
                var storeCategory = sender as StoreCategory;
                if (storeCategory != null && storeCategory.IsSelected)
                {
                    Debug.WriteLine($"Store Category Selected: {storeCategory.CategoryName}");
                    AddMultipleCategorySectionsHeader = $"Add {SelectedStoreCategories.Count} new Category Sections to Inventory";
                    IsAddMultipleCategorySectionsBtnVisible = true;
                }
                else if (storeCategory != null && !storeCategory.IsSelected)
                {
                    Debug.WriteLine($"Store Category Deselected: {storeCategory.CategoryName}");
                    SelectedStoreCategories.Remove(storeCategory);
                    AddMultipleCategorySectionsHeader = $"Add {SelectedStoreCategories.Count} new Category Sections to Inventory";
                    if (SelectedStoreCategories.Count == 0)
                    {
                        IsAddMultipleCategorySectionsBtnVisible = false;
                    }
                }                                                
            }
        }

        private void ValidateProperty(object value, [CallerMemberName] string propertyName = null)
        {
            if (propertyName == null) return;
            _errors.Remove(propertyName);

            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(this) { MemberName = propertyName };
            bool isValid = Validator.TryValidateProperty(value, context, validationResults);

            if(!isValid)
            {
                _errors[propertyName] = validationResults.Select(vr => vr.ErrorMessage).ToList();
            }

            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public bool HasErrors => _errors.Any();
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
        public IEnumerable GetErrors(string? propertyName)
        {
            if (string.IsNullOrEmpty(propertyName) || !_errors.ContainsKey(propertyName))
            {
                return null;
            }
            return _errors[propertyName];
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
