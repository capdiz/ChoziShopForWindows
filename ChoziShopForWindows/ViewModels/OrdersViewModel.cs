using ChoziShop.Data.Enums;
using ChoziShop.Data.Models;
using ChoziShop.Data.Repository.Extensions;
using ChoziShopForWindows.Converters;
using ChoziShopForWindows.Data;
using ChoziShopForWindows.Enums;
using ChoziShopForWindows.Helpers;
using ChoziShopForWindows.MerchantsApi;
using ChoziShopForWindows.models;
using ChoziShopForWindows.Services;
using ChoziShopForWindows.Views;
using HandyControl.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using PhoneNumbers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ChoziShopForWindows.ViewModels
{
    public class OrdersViewModel : INotifyPropertyChanged
    {
        private readonly IDataObjects dataObjects;
        private readonly IDialogService _dialogService;
        private readonly ITransactionService _transactionService;
        private readonly IAuthTokenProvider _authTokenProvider;

       
        private BaseApi _baseApi;

        private List<Order> _customerOrders = new List<Order>(); // Initialize to avoid null
        private ObservableCollection<SelectedCategoryProduct> _selectedCategoryProducts = new ObservableCollection<SelectedCategoryProduct>();
        private List<CategoryProduct> _lowOnStockCategoryProducts = new();
        private List<CategoryProduct> _outOfStockCategoryProducts = new();
        private List<CategoryProduct> _criticalOnStockCategoryProducts = new();

        private OrderCheckoutDialog _currentOrderCheckoutDialog;

        private CategoryProduct _selectedCatedoryProduct;
        private SelectedCategoryProduct _selectedProductItem;

        private int _customerOrdersCount;
        private string _totalSalesAmountByDay = "0 Ugx"; // Initialize to avoid null
        private string _searchQuery = string.Empty;
        private string _outOfStockHeader;
        private string _lowOnStockHeader;
        private string _criticalOnStockHeader;

        private string _mobilePaymentStatus = "Preparing order for mobile money payment (AirtelPay)...";
        private string _paymentStateText = "Initiate Payment";


        private ObservableCollection<CategoryProduct> _categoryProducts = new ObservableCollection<CategoryProduct>();
        private CancellationTokenSource _cancellationTokenSource;

        private bool _isUserQuerying;
        private bool _isSelectedCategoryProductsGridVisible = true;
        private bool _isPauseOrderButtonVisible = false;
        private bool _isLowOnStockHeaderVisible = false;
        private bool _isCriticalOnStockHeaderVisible = false;
        private bool _isOutOfStockHeaderVisible = false;
        private bool _isValidCheckout=false;
        private bool _isNoNewActionPossible = true;
        private bool _isLoadingLineVisible = false;
        private bool _isAirtelPayRequestInProgress = false;
        private bool _isMobilePaymentStatusVisible=false;



        private int _noOfItems;
        private decimal _totalAmount;
        private string _friendlyTotalAmount;
        private decimal _customerBalance = 0;
        private decimal _tenderedAmount;

        private int _airtelPhoneNumber;

        private string _failedSearchQuery = string.Empty;

        private TimeSpan _timeout= TimeSpan.FromSeconds(30);

        IServiceProvider _services;


        public OrdersViewModel(IDataObjects dataObjects, IServiceProvider services)
        {
            this.dataObjects = dataObjects;
            // Initialize the list of customer orders            
            LoadLatestCustomerOrders();
            UpdateSalesAmountCommand = new Commands.RelayCommand(_ => UpdateSales());
            SearchCategoryProductCommand = new Commands.RelayCommand(async () => await ExecuteSearchAsync());
            IsUserQuerying = false;
            AddProductCommand = new Commands.RelayCommand(_ => AddSelectedProductItem(SelectedProductItem));
            RemoveProductCommand = new Commands.RelayCommand(_ => RemoveSelectedProductItem(SelectedProductItem));
            CheckoutCommand = new Commands.RelayCommand(_ => CheckoutCustomerOrder());
            ProcessCashOrderCommand = new Commands.RelayCommand(_ => ProcessCashOrder());
            VerifyPhoneNumberCommand = new Commands.RelayCommand(_ => OnAirtelNumberVerified());
            _services = services;
            _dialogService = _services.GetRequiredService<IDialogService>();
            _transactionService =_services.GetRequiredService<ITransactionService>();
            _authTokenProvider = _services.GetRequiredService<IAuthTokenProvider>();
            _transactionService.SetAuthTokenProvider(_authTokenProvider);
        }

        public OrderCheckoutDialog CurrentOrderCheckoutDialog
        {
            get { return _currentOrderCheckoutDialog; }
            set
            {
                _currentOrderCheckoutDialog = value;
                OnPropertyChanged();
            }
        }

        public List<Order> CustomerOrders
        {
            get { return _customerOrders; }
            set
            {
                _customerOrders = value;
                OnPropertyChanged();
            }
        }

        public string TotalSalesAmountByDay
        {
            get { return _totalSalesAmountByDay; }
            set
            {
                if (_totalSalesAmountByDay != value)
                {
                    _totalSalesAmountByDay = value;
                    OnPropertyChanged();
                }
            }
        }
        public int CustomerOrdersCount
        {
            get { return _customerOrdersCount; }
            set
            {
                _customerOrdersCount = value;
                OnPropertyChanged();
            }
        }

        public string SearchQuery
        {
            get { return _searchQuery; }
            set
            {
                if (_searchQuery != value)
                {
                    _searchQuery = value;
                    if (_searchQuery.Length > 0)
                    {
                        IsUserQuerying = true;
                        IsSelectedCategoryProductsGridVisible = false;
                    }
                    else
                    {
                        IsUserQuerying = false;
                        if (SelectedCategoryProducts.Count > 0)
                        {
                            IsSelectedCategoryProductsGridVisible = true;
                            IsPauseOrderButtonVisible = true;
                        }
                        else
                        {
                            IsSelectedCategoryProductsGridVisible = false;
                        }
                    }
                    Debug.WriteLine("The search query is " + _searchQuery);
                    OnPropertyChanged();
                    DebounceSearch();
                }
            }
        }

        public bool IsUserQuerying
        {
            get { return _isUserQuerying; }
            set
            {
                _isUserQuerying = value;
                IsSelectedCategoryProductsGridVisible = false;
                OnPropertyChanged();
            }
        }

        public bool IsSelectedCategoryProductsGridVisible
        {
            get { return _isSelectedCategoryProductsGridVisible; }
            set
            {
                _isSelectedCategoryProductsGridVisible = value;
                OnPropertyChanged();
            }
        }

        public bool IsPauseOrderButtonVisible
        {
            get { return _isPauseOrderButtonVisible; }
            set
            {
                _isPauseOrderButtonVisible = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<CategoryProduct> CategoryProducts
        {
            get { return _categoryProducts; }
            set
            {
                _categoryProducts = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<SelectedCategoryProduct> SelectedCategoryProducts
        {
            get { return _selectedCategoryProducts; }
            set
            {
                _selectedCategoryProducts = value;
                _selectedCategoryProducts.CollectionChanged += (s, e) => UpdateItemNumbers();
                UpdateItemNumbers();
               
                OnPropertyChanged();
            }
        }

        public int NoOfItems
        {
            get { return _noOfItems; }
            set
            {
                _noOfItems = value;
                OnPropertyChanged();
            }
        }

        public decimal TotalAmount
        {
            get { return _totalAmount; }
            set
            {
                _totalAmount = value;
                OnPropertyChanged();
            }
        }

        public decimal CustomerBalance
        {
            get { return _customerBalance; }
            set
            {
                _customerBalance = value;
                OnPropertyChanged();
            }
        }

        public decimal TenderedAmount
        {
            get { return _tenderedAmount; }
            set
            {
                _tenderedAmount = value;
                if (_tenderedAmount >= TotalAmount)
                {
                    CustomerBalance = _tenderedAmount - TotalAmount;
                    _isValidCheckout = true;
                }
                else
                {
                    CustomerBalance = 0;
                    _isValidCheckout = false;
                }
               OnPropertyChanged();
            }
        }

        public int AirtelPhoneNumber
        {
            get { return _airtelPhoneNumber; }
            set
            {
                _airtelPhoneNumber = value;
                OnPropertyChanged();
            }
        }

        public string FriendlyTotalAmount
        {
            get { return _friendlyTotalAmount; }
            set
            {
                _friendlyTotalAmount = value;
                OnPropertyChanged();
            }
        }

        public string LowOnStockHeader
        {
            get { return _lowOnStockHeader; }
            set
            {
                _lowOnStockHeader = value;
                OnPropertyChanged();
            }
        }

        public string CriticalOnStockHeader
        {
            get { return _criticalOnStockHeader; }
            set
            {
                _criticalOnStockHeader = value;
                OnPropertyChanged();
            }
        }

        public string OutOfStockHeader
        {
            get { return _outOfStockHeader; }
            set
            {
                _outOfStockHeader = value;
                OnPropertyChanged();
            }
        }

        public bool IsOutOfStockHeaderVisible
        {
            get { return _isOutOfStockHeaderVisible; }
            set
            {
                _isOutOfStockHeaderVisible = value;
                OnPropertyChanged();
            }
        }

        public bool IsLowOnStockHeaderVisible
        {
            get { return _isLowOnStockHeaderVisible; }
            set
            {
                _isLowOnStockHeaderVisible = value;
                OnPropertyChanged();
            }
        }

        public bool IsCriticalOnStockHeaderVisible
        {
            get { return _isCriticalOnStockHeaderVisible; }
            set
            {
                _isCriticalOnStockHeaderVisible = value;
                OnPropertyChanged();
            }
        }

        public bool IsNoNewActionPossible
        {
            get { return _isNoNewActionPossible; }
            set
            {
                _isNoNewActionPossible = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoadingLineVisible
        {
            get { return _isLoadingLineVisible; }
            set
            {
                _isLoadingLineVisible = value;
                OnPropertyChanged();
            }
        }

        public bool IsAirtelPayRequestInProgress
        {
            get { return _isAirtelPayRequestInProgress; }

            set => SetField(ref _isAirtelPayRequestInProgress, value);
        }

        public TimeSpan TimeOut
        {
            get { return _timeout; }
            set
            {
                SetField(ref _timeout, value);
            }
        }

        public bool IsMobilePaymentStatusVisible
        {
            get { return _isMobilePaymentStatusVisible; }
            set
            {
                _isMobilePaymentStatusVisible = value;
                OnPropertyChanged();
            }
        }

        public string MobilePaymentStatus
        {
            get { return _mobilePaymentStatus; }
            set
            {
                _mobilePaymentStatus = value;
                OnPropertyChanged();
            }
        }

        public string PaymentStateText
        {
            get { return _paymentStateText; }
            set
            {
                _paymentStateText = value;
                OnPropertyChanged();
            }
        }


        private void _selectedCategoryProducts_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public CategoryProduct? SelectedCategoryProduct
        {
            get { return _selectedCatedoryProduct; }
            set
            {
                _selectedCatedoryProduct = value;
                AddOrUpdateSelectedCategoryProducts(_selectedCatedoryProduct);
                IsUserQuerying = false;
                SearchQuery = string.Empty;
                IsSelectedCategoryProductsGridVisible = true;
                IsPauseOrderButtonVisible = true;
                OnPropertyChanged();
            }
        }

        public SelectedCategoryProduct SelectedProductItem
        {
            get { return _selectedProductItem; }
            set
            {
                _selectedProductItem = value;
                Debug.WriteLine("Selected product item: " + _selectedProductItem?.ItemName);
                OnPropertyChanged();
            }
        }

        public void UpdateSales()
        {
            // Calculate the total sales amount by day            
            TotalSalesAmountByDay = "Uganda cranes Ugx";
        }

        public ICommand UpdateSalesAmountCommand { get; }

        public ICommand SearchCategoryProductCommand { get; }

        public ICommand AddProductCommand { get; }
        public ICommand RemoveProductCommand { get; }
        public ICommand CheckoutCommand { get; }

        public ICommand ProcessCashOrderCommand { get; }
        public ICommand ProcessMobilePaymentCommand { get; }
        public ICommand VerifyPhoneNumberCommand { get; }


        async void LoadLatestCustomerOrders()
        {
            _customerOrders = await dataObjects.ShopOrders.SortOrderByDateAndOrderStatus(DateTime.Now, CustomerOrderStatus.Closed, SortDirection.Descending);
            CustomerOrdersCount = _customerOrders.Count + 1;
        }

        private void AddOrUpdateSelectedCategoryProducts(CategoryProduct categoryProduct)
        {
            if (categoryProduct == null) return;

            var existing = SelectedCategoryProducts.FirstOrDefault(product => product.CategoryProduct?.Id == categoryProduct.Id);
            if (existing != null)
            {
                existing.UnitCount++;
                existing.TotalPrice = existing.UnitCount * existing.UnitPrice;
            }
            else
            {
                SelectedCategoryProducts.Add(new SelectedCategoryProduct
                {
                    ItemName = categoryProduct.ProductName,
                    CategoryProduct = categoryProduct,
                    UnitCount = 1,
                    UnitPrice = categoryProduct.UnitCost,
                    TotalPrice = categoryProduct.UnitCost
                });

            }
            SelectedCategoryProducts.CollectionChanged += (s, e) => UpdateItemNumbers();
            TotalAmount = SelectedCategoryProducts.Sum(x => x.TotalPrice);
            UpdateItemNumbers();
            if (SelectedCategoryProducts.Sum(x => x.UnitCount) > 0)
            {
                NoOfItems = SelectedCategoryProducts.Sum(x => x.UnitCount);
            }
            else
            {
                NoOfItems = 0;
            }
            categoryProduct.Units--;
            var inventoryStatus = InventoryStatus.GetCategoryProductInventoryStatus(categoryProduct.Units);
            Debug.WriteLine("Inventory status: " + inventoryStatus + " No of units: " + categoryProduct.Units);
            switch (inventoryStatus)
            {
                case Enums.ProductInventoryStatus.Empty:
                    if (!_outOfStockCategoryProducts.Contains(categoryProduct))
                    {
                        _outOfStockCategoryProducts.Add(categoryProduct);
                    }
                    OutOfStockHeader = GetStockStatusHeader(_outOfStockCategoryProducts, 0);
                    if (OutOfStockHeader == string.Empty)
                    {
                        IsOutOfStockHeaderVisible = false;
                    }
                    else
                    {
                        IsOutOfStockHeaderVisible = true;
                    }
                    return;
                case Enums.ProductInventoryStatus.Critical:
                    if (!_criticalOnStockCategoryProducts.Contains(categoryProduct))
                    {
                        _criticalOnStockCategoryProducts.Add(categoryProduct);
                    }
                    CriticalOnStockHeader = GetStockStatusHeader(_criticalOnStockCategoryProducts, 1);
                    if(CriticalOnStockHeader == string.Empty)
                    {
                        IsCriticalOnStockHeaderVisible = false;
                    }
                    else
                    {
                        IsCriticalOnStockHeaderVisible = true;
                    }
                    break;
                case Enums.ProductInventoryStatus.Low:
                    if (!_lowOnStockCategoryProducts.Contains(categoryProduct))
                    {
                        _lowOnStockCategoryProducts.Add(categoryProduct);
                    }
                    LowOnStockHeader = GetStockStatusHeader(_lowOnStockCategoryProducts, 2);
                    if(LowOnStockHeader == string.Empty)
                    {
                        IsLowOnStockHeaderVisible = false;
                    }
                    else
                    {
                        IsLowOnStockHeaderVisible = true;
                    }
                    break;
            }
        }

        private void RemoveSelectedProductItem(SelectedCategoryProduct selectedProductItem)
        {
            Debug.WriteLine("Removing selected product item: " + selectedProductItem?.ItemName);
            if (selectedProductItem == null) return;
            var existing = SelectedCategoryProducts.FirstOrDefault(product => product.CategoryProduct.OnlineCategoryProductId == selectedProductItem.CategoryProduct.OnlineCategoryProductId);
            if (existing != null)
            {
                if (SelectedProductItem.UnitCount > 1)
                {
                    Debug.WriteLine("Decreasing unit count for existing item: " + existing.ItemName);
                    existing.UnitCount--;
                    existing.TotalPrice = existing.UnitCount * existing.UnitPrice;
                    NoOfItems = SelectedCategoryProducts.Sum(x => x.UnitCount);

                    existing.CategoryProduct.Units++;
                    var inventoryStatus = InventoryStatus.GetCategoryProductInventoryStatus(existing.CategoryProduct.Units);
                    Debug.WriteLine("Inventory status: " + inventoryStatus + " No of units: " + existing.CategoryProduct.Units);
                    switch (inventoryStatus)
                    {
                        case Enums.ProductInventoryStatus.Empty:
                            DataListHelper.ReplaceOrAdd(_outOfStockCategoryProducts, x => x.Id == existing.CategoryProduct.Id, existing.CategoryProduct);
                            OutOfStockHeader = GetStockStatusHeader(_outOfStockCategoryProducts, 0);
                            if (OutOfStockHeader == string.Empty)
                            {
                                IsOutOfStockHeaderVisible = false;
                            }
                            else
                            {
                                IsOutOfStockHeaderVisible = true;
                            }
                            return;
                        case Enums.ProductInventoryStatus.Critical:
                            DataListHelper.ReplaceOrAdd(_criticalOnStockCategoryProducts, x => x.Id == existing.CategoryProduct.Id, existing.CategoryProduct);
                            CriticalOnStockHeader = GetStockStatusHeader(_criticalOnStockCategoryProducts, 1);
                            // Remove the product from the empty stock list if it exists
                            if (_outOfStockCategoryProducts.Contains(existing.CategoryProduct))
                            {                               
                                _outOfStockCategoryProducts.Remove(existing.CategoryProduct);
                                OutOfStockHeader = GetStockStatusHeader(_outOfStockCategoryProducts, 0);
                                if (OutOfStockHeader == string.Empty)
                                {
                                    IsOutOfStockHeaderVisible = false;
                                }
                                else
                                {
                                    IsOutOfStockHeaderVisible = true;
                                }
                            }
                            if (CriticalOnStockHeader == string.Empty)
                            {
                                IsCriticalOnStockHeaderVisible = false;
                            }
                            else
                            {
                                IsCriticalOnStockHeaderVisible = true;
                            }
                            break;
                        case Enums.ProductInventoryStatus.Low:
                            DataListHelper.ReplaceOrAdd(_lowOnStockCategoryProducts, x => x.Id == existing.CategoryProduct.Id, existing.CategoryProduct);
                            LowOnStockHeader = GetStockStatusHeader(_lowOnStockCategoryProducts, 2);
                            // Remove the product from the critical stock list if it exists
                            if (_criticalOnStockCategoryProducts.Contains(existing.CategoryProduct))
                            {
                                _criticalOnStockCategoryProducts.Remove(existing.CategoryProduct);                            
                                CriticalOnStockHeader = GetStockStatusHeader(_criticalOnStockCategoryProducts, 1);
                                if (CriticalOnStockHeader == string.Empty)
                                {
                                    IsCriticalOnStockHeaderVisible = false;
                                }
                                else
                                {
                                    IsCriticalOnStockHeaderVisible = true;
                                }
                            }
                            if (LowOnStockHeader == string.Empty)
                            {
                                IsLowOnStockHeaderVisible = false;
                            }
                            else
                            {
                                IsLowOnStockHeaderVisible = true;
                            }
                            break;
                    }
                }
                else
                {
                    Debug.WriteLine("Removing item from selected category products: " + existing.ItemName);
                    SelectedCategoryProducts.Remove(existing);

                    NoOfItems = SelectedCategoryProducts.Sum(x => x.UnitCount);
                    UpdateItemNumbers();
                    SearchQuery = string.Empty;
                    if (SelectedCategoryProducts.Count == 0)
                    {
                        IsSelectedCategoryProductsGridVisible = false;
                        IsPauseOrderButtonVisible = false;
                        IsLowOnStockHeaderVisible = false;
                        IsCriticalOnStockHeaderVisible = false;
                        IsOutOfStockHeaderVisible = false;
                    }

                    if (_outOfStockCategoryProducts.Contains(existing.CategoryProduct))
                    {
                        _outOfStockCategoryProducts.Remove(existing.CategoryProduct);
                        OutOfStockHeader = GetStockStatusHeader(_outOfStockCategoryProducts, 0);
                        if (OutOfStockHeader == string.Empty)
                        {
                            IsOutOfStockHeaderVisible = false;
                        }
                        else
                        {
                            IsOutOfStockHeaderVisible = true;
                        }
                    }
                    else if (_criticalOnStockCategoryProducts.Contains(existing.CategoryProduct))
                    {
                        _criticalOnStockCategoryProducts.Remove(existing.CategoryProduct);
                        CriticalOnStockHeader = GetStockStatusHeader(_criticalOnStockCategoryProducts, 1);
                        if (CriticalOnStockHeader == string.Empty)
                        {
                            IsCriticalOnStockHeaderVisible = false;
                        }
                        else
                        {
                            IsCriticalOnStockHeaderVisible = true;
                        }
                    }
                    else if (_lowOnStockCategoryProducts.Contains(existing.CategoryProduct))
                    {
                        _lowOnStockCategoryProducts.Remove(existing.CategoryProduct);
                        LowOnStockHeader = GetStockStatusHeader(_lowOnStockCategoryProducts, 2);
                        if (LowOnStockHeader == string.Empty)
                        {
                            IsLowOnStockHeaderVisible = false;
                        }
                        else
                        {
                            IsLowOnStockHeaderVisible = true;
                        }
                    }
                }
                TotalAmount = SelectedCategoryProducts.Sum(x => x.TotalPrice);
            }
        }

        private void AddSelectedProductItem(SelectedCategoryProduct selectedProductItem)
        {
            Debug.WriteLine("Adding selected product item: " + selectedProductItem?.ItemName);
            if (selectedProductItem == null) return;
            var existing = SelectedCategoryProducts.FirstOrDefault(product => product.CategoryProduct.OnlineCategoryProductId == selectedProductItem.CategoryProduct.OnlineCategoryProductId);
            if (existing != null)
            {
                existing.UnitCount++;
                existing.TotalPrice = existing.UnitCount * existing.UnitPrice;
                TotalAmount = SelectedCategoryProducts.Sum(x => x.TotalPrice);
                NoOfItems = SelectedCategoryProducts.Sum(x => x.UnitCount);

                existing.CategoryProduct.Units--;
                var inventoryStatus = InventoryStatus.GetCategoryProductInventoryStatus(existing.CategoryProduct.Units);
                Debug.WriteLine("Inventory status: " + inventoryStatus + " No of units: " + existing.CategoryProduct.Units);
                switch (inventoryStatus)
                {
                    case Enums.ProductInventoryStatus.Empty:
                        DataListHelper.ReplaceOrAdd(_outOfStockCategoryProducts, x => x.Id == existing.CategoryProduct.Id, existing.CategoryProduct);
                        OutOfStockHeader = GetStockStatusHeader(_outOfStockCategoryProducts, 0);                        
                        // Remove the product from the low on stock and critical on stock lists if it exists
                        if (_criticalOnStockCategoryProducts.Contains(existing.CategoryProduct))
                        {
                            _criticalOnStockCategoryProducts.Remove(existing.CategoryProduct);
                            CriticalOnStockHeader = GetStockStatusHeader(_criticalOnStockCategoryProducts, 1);
                            IsCriticalOnStockHeaderVisible = false;
                        }
                        else if (_lowOnStockCategoryProducts.Contains(existing.CategoryProduct))
                        {
                            _lowOnStockCategoryProducts.Remove(existing.CategoryProduct);
                            LowOnStockHeader = GetStockStatusHeader(_lowOnStockCategoryProducts, 2);
                            IsLowOnStockHeaderVisible = false;
                        }
                        if(OutOfStockHeader == string.Empty)
                        {
                            IsOutOfStockHeaderVisible = false;
                        }
                        else
                        {
                            IsOutOfStockHeaderVisible = true;
                        }
                        return;
                    case Enums.ProductInventoryStatus.Critical:
                        DataListHelper.ReplaceOrAdd(_criticalOnStockCategoryProducts, x => x.Id == existing.CategoryProduct.Id, existing.CategoryProduct);
                        CriticalOnStockHeader = GetStockStatusHeader(_criticalOnStockCategoryProducts, 1);                        
                        // Remove the product from the low on stock list if it exists
                        if (_lowOnStockCategoryProducts.Contains(existing.CategoryProduct))
                        {
                            _lowOnStockCategoryProducts.Remove(existing.CategoryProduct);
                            LowOnStockHeader = GetStockStatusHeader(_lowOnStockCategoryProducts, 2);
                            IsLowOnStockHeaderVisible = false;
                        }
                        if(CriticalOnStockHeader == string.Empty)
                        {
                            IsCriticalOnStockHeaderVisible = false;
                        }
                        else
                        {
                            IsCriticalOnStockHeaderVisible = true;
                        }
                        break;
                    case Enums.ProductInventoryStatus.Low:
                        DataListHelper.ReplaceOrAdd(_lowOnStockCategoryProducts, x => x.Id == existing.CategoryProduct.Id, existing.CategoryProduct);
                        LowOnStockHeader = GetStockStatusHeader(_lowOnStockCategoryProducts, 2);
                        if(LowOnStockHeader == string.Empty)
                        {
                            IsLowOnStockHeaderVisible = false;
                        }
                        else
                        {
                            IsLowOnStockHeaderVisible = true;
                        }
                        break;
                }
            }
        }

        private void SelectedCategoryProducts_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void UpdateItemNumbers()
        {
            for (int i = 0; i < SelectedCategoryProducts.Count; i++)
            {
                Debug.WriteLine(SelectedCategoryProducts[i].CategoryProduct.ProductName + " " + SelectedCategoryProducts[i].UnitCount);
                SelectedCategoryProducts[i].ItemNo = i + 1;
            }
        }

        private async Task ExecuteSearchAsync()
        {
            try
            {
                var searchQuery = SearchQuery.ToLower();
                var categoryProducts = await dataObjects.SearchCategoryProductsAsync(searchQuery);
                CategoryProducts = new ObservableCollection<CategoryProduct>(categoryProducts);
                if (CategoryProducts.Count == 0 && SelectedCategoryProducts.Count > 0)
                {
                    IsSelectedCategoryProductsGridVisible = true;
                    IsPauseOrderButtonVisible = true;
                }
           
                Debug.WriteLine($"Search query: {searchQuery}, Results: {CategoryProducts.Count}");
            }
            catch (Exception ex)
            {                                              
                Debug.WriteLine("Error searching category products: " + ex.Message+ " With query: "+SearchQuery.ToLower());
            }
        }

        private async Task OnAirtelNumberVerified()
        {
            if (IsValidAirtelNumber(AirtelPhoneNumber.ToString()))
            {
                var result = _dialogService
                    .ShowMessageBox($"You are initiating a withdraw of {CurrencyFormatter.FormatToUgxCurrency(TotalAmount)} from {AirtelPhoneNumber}. " +
                    $"Would you like to proceed with this request?",
                    "Verify Initiation Request!",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    IsNoNewActionPossible = false;
                    IsMobilePaymentStatusVisible = true;
                    PaymentStateText = "Processing order...";
                    // Process customer order and initiate payment request
                    var order = await ProcessCustomerOrder();
                    if (order != null)
                    {
                        await InitiateAirtelPayRequest(order);
                    }
                    else
                    {
                        MessageBox.Show("Failed to process customer order.", "Order Checkout Failed!");
                    }

                }
            }
            else
            {
                MessageBox.Show("Number provided is't valid on the Airtel network.", "Invalid Phone Number!");
            }
        }

        private async Task<Order?> ProcessCustomerOrder()
        {
            IsNoNewActionPossible = false;
            var order = new Order()
            {
                StoreId = 171,
                PreferredPaymentMode = 1,
                PaymentStatus = 0,
                OrderStatus = 2,
                TotalAmountCents = 400,
                TotalAmountCurrency = "UGX",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                OfflineUpdatedAt = DateTime.Now,
                OrderProductItems = SelectedCategoryProducts.Select(x => x.CategoryProduct).ToList()
            };
            // Get currently logged in user
            var currentUserAccount = _authTokenProvider.GetCurrentMerchantAccount();
            if (currentUserAccount != null)
            {
                IsLoadingLineVisible = true;
                IsMobilePaymentStatusVisible = true;
                _baseApi = new BaseApi(currentUserAccount.AuthToken);
                // start processing order 
                var serializedOrder = await _baseApi.ProcessMobileMoneyOrder(order, "Merchant");
                if (serializedOrder != null)
                {
                    Debug.WriteLine("Order successfully saved to API with OnlineId: " + serializedOrder.Id);
                    // save order to db
                    order.OnlineOrderId = serializedOrder.Id;
                    order.OrderProcessedById = currentUserAccount.OnlineMerchantId;

                    var savedOrder = await dataObjects.ShopOrders.SaveAndReturnEntityAsync(order);
                    return savedOrder;
                }
            }
            return null;
        }

        private async Task InitiateAirtelPayRequest(Order order)
        {
          
            // Get payment auth token first before making request
            var paymentAuth = await dataObjects.GetPaymentAuth();
            if (paymentAuth != null)
            {
                // Change from user auth token to payment auth token
                _baseApi = new BaseApi(paymentAuth.AuthToken);
                _authTokenProvider.SetPaymentAuthToken(paymentAuth.AuthToken);               
                Debug.WriteLine("Payment auth token found: " + paymentAuth.AuthToken);
                await InitiatePinRequest(order);
            }
            else
            {
                // create a payment auth token first and then proceed to initiate payment request
                Debug.WriteLine("Creating payment auth token");
                paymentAuth = await CreatePaymentAuth();
                if (paymentAuth != null)
                {
                    // change from user auth token to payment auth token
                    _baseApi = new BaseApi(paymentAuth.AuthToken);
                    await InitiatePinRequest(order);
                }

            }
        }

        private async Task InitiatePinRequest(Order order)
        {
            var collectionRequestResponse = await _baseApi.InitiatePaymentRequest(AirtelPhoneNumber.ToString(), TotalAmount);
            if (collectionRequestResponse != null && collectionRequestResponse.CollectionRequest != null)
            {
                IsAirtelPayRequestInProgress = true;
                Debug.WriteLine("AirtelPayCollectionRequestDetails: Transaction Id: " + collectionRequestResponse.CollectionRequest.TransactionId + ". Status: " +
                    collectionRequestResponse.CollectionRequest.Status + ", Phone Number: " + collectionRequestResponse.Msisdn);
                var airtelPayCollection = new AirtelPayCollection()
                {
                    AirtelPayCollectionRequestId = collectionRequestResponse.CollectionRequest.AirtelPayCollectionRequestId,
                    CustomerOrderId = order.OnlineOrderId,
                    Amount = TotalAmount,
                    Msisdn = AirtelPhoneNumber.ToString(),
                    TransactionId = collectionRequestResponse.CollectionRequest.TransactionId,
                    Status = collectionRequestResponse.CollectionRequest.Status,
                    PaymentInitiatedAt = DateTime.Now
                };
                var savedAirtelPayCollection = await dataObjects.AirtelCollections.SaveAndReturnEntityAsync(airtelPayCollection);
                if (savedAirtelPayCollection != null && savedAirtelPayCollection.Id > 0)
                {
                    Debug.WriteLine("AirtelPay collection request successfully saved to local database with Id: " + savedAirtelPayCollection.Id);
                    // Start polling for transaction status
                    IsMobilePaymentStatusVisible = true;
                    PaymentStateText = "Waiting...";
                    MobilePaymentStatus = $"Payment request initiated. PIN request sent to customer on number {airtelPayCollection.Msisdn}...";
                    await StartStatusPolling(savedAirtelPayCollection);
                }

            }
        }

        private async Task<PaymentAuth?> CreatePaymentAuth()
        {
            _baseApi = new BaseApi(_authTokenProvider.GetCurrentMerchantAccount().AuthToken);
            Guid uuid = Guid.NewGuid();
            string accessKeyId = uuid.ToString();
            string email = "mauricekus@gmail.com";
            var paymentAuthRequest = await _baseApi.CreatePaymentAuth(email, accessKeyId);
            if (paymentAuthRequest != null)
            {
                var paymentAuth = new PaymentAuth
                {
                    AuthToken = paymentAuthRequest.AuthToken,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                
                _authTokenProvider.SetPaymentAuthToken(paymentAuth.AuthToken);
                Debug.WriteLine("Payment auth token: " + paymentAuth.AuthToken);
                return await dataObjects.CreatePaymentAuth(paymentAuth);
            }
            return null;
        }

        private async Task StartStatusPolling(AirtelPayCollection airtelPayCollection)
        {
            try
            {
                MobilePaymentStatus = "Awaiting for customer PIN confirmation...";
                IsMobilePaymentStatusVisible = true;
                _cancellationTokenSource = new CancellationTokenSource();

                var timeoutToken = new CancellationTokenSource(_timeout).Token;
                var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token, timeoutToken).Token;

                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    Debug.WriteLine("Polling for transaction status...");
                    linkedToken.ThrowIfCancellationRequested();

                    var result = await _transactionService.CheckTransactionStatusAsync(airtelPayCollection, dataObjects);
                    MobilePaymentStatus = result switch
                    {
                        TransactionStatus.TIP => "Payment request is pending. Awaiting customer PIN confirmation...",
                        TransactionStatus.TS => "Customer successfully confirmed payment.",
                        TransactionStatus.TF => "Payment request failed. Please try again.",
                        TransactionStatus.INVALID_PIN => "PIN authorization failed.",
                        TransactionStatus.INSUFFICIENT_FUNDS => "Insufficient funds in customer account.",
                        TransactionStatus.TTO => "Transaction timed out. Please try again.",
                        _ => "Unknown transaction status."
                    };
                    if (result != TransactionStatus.TIP)
                    {
                        switch (result)
                        {
                            case TransactionStatus.TS:
                                transactionSuccessful();
                                break;
                            case TransactionStatus.TF:
                                transactionFailed();
                                break;
                            case TransactionStatus.INVALID_PIN:
                                invalidPin();
                                break;
                            case TransactionStatus.INSUFFICIENT_FUNDS:
                                insufficientFunds();
                                break;
                            case TransactionStatus.TTO:
                                transactionTimedOut();
                                break;
                        }
                        Debug.WriteLine("Transaction status: " + result);                        
                        _cancellationTokenSource.Cancel();
                        break;
                    }

                    await Task.Delay(5000, linkedToken); // Poll after every 5 seconds
                }
            }
            catch (TaskCanceledException)
            {
                // Handle cancellation
                Debug.WriteLine("Status polling cancelled.");
                CancelPolling();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error during status polling: " + ex.Message);
                CancelPolling();
            }
        }

        private void CancelPolling()
        {
            _cancellationTokenSource?.Cancel();
            transactionTimedOut();
        }

        private void transactionSuccessful()
        {
            if (_cancellationTokenSource != null)
            {
                PaymentStateText = "Completed!";
                AirtelPhoneNumber = 0;
                IsAirtelPayRequestInProgress = false;
                IsLoadingLineVisible = false;
                IsMobilePaymentStatusVisible = false;
                IsNoNewActionPossible = true;
                // Update UI and reset to a new order
                CurrentOrderCheckoutDialog?.close();
                SelectedCategoryProducts.Clear();
                SelectedCategoryProduct = null;
                IsSelectedCategoryProductsGridVisible = false;
                IsPauseOrderButtonVisible = false;
                IsLowOnStockHeaderVisible = false;
                IsCriticalOnStockHeaderVisible = false;
                IsOutOfStockHeaderVisible = false;
                NoOfItems = 0;
                TotalAmount = 0;
                MessageBox.Show("Payment request was successful.", "Payment Successful!");
            }
        }

        private void invalidPin()
        {
            if (_cancellationTokenSource != null)
            {
                PaymentStateText = "Try again?";
                IsLoadingLineVisible = false;
                IsMobilePaymentStatusVisible = true;
                MobilePaymentStatus = "PIN authorization failed. Please try again.";
                IsNoNewActionPossible = true;
                MessageBox.Show("PIN authorization failed. Please try again.", "Invalid PIN!");
            }
        }

        private void insufficientFunds()
        {
            if (_cancellationTokenSource != null)
            {
                PaymentStateText = "Try again?";
                IsLoadingLineVisible = false;
                IsMobilePaymentStatusVisible = true;
                MobilePaymentStatus = "Insufficient funds in customer account. Please try again.";
                IsNoNewActionPossible = true;
                MessageBox.Show("Insufficient funds in customer account. Please try again.", "Insufficient Funds!");
            }
        }

        private void transactionFailed()
        {
            if (_cancellationTokenSource != null)
            {
                PaymentStateText = "Try again?";
                IsLoadingLineVisible = false;
                IsMobilePaymentStatusVisible = true;
                MobilePaymentStatus = "Payment request failed. Please try again.";
                IsNoNewActionPossible = true;
                MessageBox.Show("Payment request failed. You can try again.", "Payment Failed!");
            }
        }

        private void transactionTimedOut()
        {
            PaymentStateText = "Try again?";
            IsLoadingLineVisible = false;
            IsMobilePaymentStatusVisible = true;
            MobilePaymentStatus = "Transaction timed out. Please try again.";
            IsNoNewActionPossible = true;
            MessageBox.Show("Transaction timed out. Please try again.", "Transaction Timed Out!");
        }

        private void ProcessCashOrder()
        {
            if (_isValidCheckout)
            {
                MessageBox.Show("Cash order processing is not yet implemented.");
            }
            else
            {
                MessageBox.Show("Order processing failed. Verify tendered amount and try again.", "INVALID ORDER CHECKOUT!");
            }
        }

        private void DebounceSearch()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;
            Task.Delay(300, token).ContinueWith(async t =>
            {
                if (!t.IsCanceled)
                {
                    await ExecuteSearchAsync();
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        /**
         * 0. When empty
         * 1. When critical
         * 2. When low
         */
        private string GetStockStatusHeader(List<CategoryProduct> categoryProducts, int status)
        {
            if (categoryProducts == null || categoryProducts.Count == 0)
            {
                return string.Empty;
            }

            string[] productNames = categoryProducts.Select(p => p.ProductName).ToArray();
            string header = string.Join(", ", productNames.Take(productNames.Length - 1)) +
                (productNames.Length > 1 ? " and " : "")
                + productNames.LastOrDefault();
            switch (status)
            {
                case 0:
                    return $"* {header} {(productNames.Length == 1 ? "is" : "are")} out of stock.";
                case 1:
                    return $"* {header} {(productNames.Length == 1 ? "is" : "are")} critically low on stock.";
                case 2:
                    return $"* {header} {(productNames.Length == 1 ? "is" : "are")} low on stock.";
            }

            return string.Empty;
        }

        private void CheckoutCustomerOrder()
        {
            if(_outOfStockCategoryProducts.Count > 0)
            {
                MessageBox.Show("You cannot checkout an order with out of stock items.", "Order Checkout Failed!");
                return;
            }
            else
            {
                ShowOrderCheckoutDialog(); return;
            }
        }

        private void ShowOrderCheckoutDialog()
        {
            var ordersViewModer = _services.GetRequiredService<OrdersViewModel>();
            OrderCheckoutDialog checkoutDialog = new OrderCheckoutDialog(ordersViewModer);
            CurrentOrderCheckoutDialog = checkoutDialog;
            HandyControl.Controls.Dialog.Show(checkoutDialog);
        }

        private bool IsValidAirtelNumber(string airtelNumber)
        {
            try
            {
                PhoneNumberUtil phoneNumberUtil = PhoneNumberUtil.GetInstance();
                PhoneNumber parsedNumber = phoneNumberUtil.Parse(airtelNumber, "UG");
                if (phoneNumberUtil.IsValidNumber(parsedNumber))
                    return true;
            }
            catch (NumberParseException ex)
            {
            }
            return false;
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
