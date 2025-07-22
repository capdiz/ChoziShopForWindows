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
using ChoziShopSharedConnectivity.Shared;
using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Interactivity;
using HandyControl.Tools;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PhoneNumbers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.IO.Pipes;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MessageBox = System.Windows.MessageBox;

namespace ChoziShopForWindows.ViewModels
{
    public class OrdersViewModel : INotifyPropertyChanged
    {
        private readonly IDataObjects dataObjects;
        private readonly IDialogService _dialogService;
        private readonly ITransactionService _transactionService;
        private readonly IAuthTokenProvider _authTokenProvider;
        private readonly InternetConnectivityMonitorService _internetConnectivityMonitorService;

        private int _loggedInUserAccountType;
        

        private BaseApi _baseApi;

        private ObservableCollection<Order> _customerOrders=new ObservableCollection<Order>(); // Initialize to avoid null
        private ObservableCollection<SelectedCategoryProduct> _selectedCategoryProducts = new ObservableCollection<SelectedCategoryProduct>();
        private List<CategoryProduct> _lowOnStockCategoryProducts = new();
        private List<CategoryProduct> _outOfStockCategoryProducts = new();
        private List<CategoryProduct> _criticalOnStockCategoryProducts = new();

        private List<CategoryProduct> _inventoryCategoryProducts;

        private ObservableCollection<ClosedOrder> _closedOrders = new();
        private ObservableCollection<Order> _pendingOrders = new();

        private OrderCheckoutDialog _currentOrderCheckoutDialog;

        private CategoryProduct _selectedCategoryProduct;
        private SelectedCategoryProduct _selectedProductItem;

        private ObservableCollection<Order> _todaysOrders = new();
        private ObservableCollection<Order> _weeklyOrders = new();
        private ObservableCollection<Order> _monthlyOrders = new();

        private string _topSellingItems;
        private List<TopSellingCategoryProduct> _topSellingCategoryproducts;
        private List<TopSellingCategory> _topSellingCategories;
        private List<StockItem> _stockItems = new();

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
        private bool _isInternetConnected;
        private bool _isCheckoutButtonVisible = true; // Default to true, can be set to false when no items are selected
        private bool _isOrdersDataGridVisible = false;
        private bool _isPendingOrdersDataGridVisible = false;
        private bool _isHeldOrdersDataGridVisible = false;

        private bool _isCheckoutButtonEnabled = false;
        private bool _isSalesDataGridVisible = false;   
        private bool _isTopSellingItemsDataGridVisible = false;
        private bool _isTopSellingCategoriesDataGridVisible = false;
        private bool _isInventoryTrackerDataGridVisible = false;


        private string _selectedActionHeader;

        private int _noOfItems;
        private decimal _totalAmount;

        private string _friendlyTotalAmount;
        
        private decimal _customerBalance = 0;
        private decimal _tenderedAmount;
        private decimal _totalDailySalesAmount;
        private decimal _selectedSalesAmount;
        private decimal _totalCashSalesAmount;
        private decimal _totalMobileMoneySalesAmount;

        private int _closedOrdersCount;

        private int _airtelPhoneNumber;

        // 0. Today's actions
        // 1. Week's actions
        // 2. Month's actions
        private int _selectedAction = 0;

        private TimeSpan _timeout= TimeSpan.FromSeconds(30);

        IServiceProvider _services;

        private Store? CurrentStore;

        // Can only be set when returning from a PausedOrder
        private Order? _currentOrder;
        
        private ObservableCollection<PausedOrder> _pausedOrders = new(); // Initialize to avoid null
        
        private Order _pausedOrder;
        private PausedOrder _selectedPausedOrder;
        private ClosedOrder _selectedClosedOrder;

        private int _pausedOrdersCount;
        private int _outOfStockCount;
        private int _criticalOnStockCount;
        private int _lowOnStockCount;

        private object? _selectedOrderControlTab;
        private object? _currentOrderControlTab;

        private bool _isHeldOrdersBadgeVisible = false;
        private bool _isHeldOrdersHeaderVisible = true;


        private TabItem _currentOrderTabItem;

        private string? _friendlyOrderItemsList;
        private string? _heldOrdersTitle;
        private string? _closedOrdersTitle;
        private string? _friendlyClosedOrdersItemsList;

        private ObservableCollection<Sale> _sales = new ObservableCollection<Sale>();
        public ObservableCollection<DateRangeOption> DateRangeOptions { get; }
        private DateRangeOption _selectedDateRange;
        private DateRangeOption _selectedProductDateRange;
        private DateRangeHelper dateRangeHelper;

        private List<InventoryTracker> _inventoryTrackers = new();

        private StockItem _selectedStockItem;
        EditProductDialog editProductDialog;
        RestockItemDialog restockItemDialog;

        private string _salesTypeHeader;
        private string _totalSalesAmountHeader;
        private decimal _totalSalesAmountValue;

        private string _orderProcessedByAccountType;
       
        // 0. Daily sales
        // 1. Weekly sales
        // 2. Monthly sales
        private int _chartType = 0;

        private ConcurrentDictionary<long, string> _userLookupCache = new ConcurrentDictionary<long, string>();

        public OrdersViewModel(IDataObjects dataObjects, IServiceProvider services)
        {
            this.dataObjects = dataObjects;
            // Load user accounts on device
           _ =  LoadUserAccounts();
            // Load all inventory category products
            LoadInventoryCategoryProducts();
            // Initialize the list of customer orders            
            LoadLatestCustomerOrders();
            // Initialize the list of top selling products
            LoadTopSellingProducts();
            LoadTopSellingCategories();
            LoadPendingOrders();

            SelectedAction = 0;
            IsTopSellingItemsDataGridVisible = false;
            IsTopSellingCategoriesDataGridVisible = false;           
            IsPendingOrdersDataGridVisible = false;

            UpdateSalesAmountCommand = new Commands.RelayCommand(_ => UpdateSales());
            SearchCategoryProductCommand = new Commands.RelayCommand(async () => await ExecuteSearchAsync());
            IsUserQuerying = false;
            AddProductCommand = new Commands.RelayCommand(_ => AddSelectedProductItem(SelectedProductItem));
            RemoveProductCommand = new Commands.RelayCommand(_ => RemoveSelectedProductItem(SelectedProductItem));
            CheckoutCommand = new Commands.RelayCommand(_ => CheckoutCustomerOrder());
            ProcessCashOrderCommand = new Commands.RelayCommand(_ => ProcessCashOrder());
            VerifyPhoneNumberCommand = new Commands.RelayCommand(async _ => await OnAirtelNumberVerified());
            PauseCurrentOrderCommand = new Commands.RelayCommand(async _ => await PauseCurrentOrder());
            ResumeOrderCommand = new Commands.RelayCommand(_ => ResumeOrder());
            CancelOrderCommand = new Commands.RelayCommand(_ => CancelOrder());
            ShowTodaysSalesCommand = new Commands.RelayCommand(_ => ShowTodaysSales());
            ShowThisWeeksSalesCommand = new Commands.RelayCommand(_ => ShowThisWeeksSales());
            ShowThisMonthsSalesCommand = new Commands.RelayCommand(_ => ShowThisMonthsSales());
            GenerateChartCommand = new Commands.RelayCommand(_ => ShowChartDialog());
            ShowClosedOrdersCommand = new Commands.RelayCommand(_ => ShowClosedOrders());
            ShowHeldOrdersCommand =new Commands.RelayCommand(_ => ShowPausedOrders());
            ShowPaymentsCommand = new Commands.RelayCommand(_ => ShowPaymentMethods());
            ShowPaymentModesChartCommand = new Commands.RelayCommand(_ => ShowPaymentsChartDialog());
            ShowTopSellingProductsCommand=new Commands.RelayCommand(_ => ShowTopSellingProducts());
            ShowTopCategoriesCommand = new Commands.RelayCommand(_ => ShowTopSellingCategories());
            ShowInventoryTrackerCommand = new Commands.RelayCommand(_ => ShowInventoryTracker());
            StockActionCommand = new Commands.RelayCommand(param => ShowSelectedStockAction(param));
            InventoryLogCommand = new Commands.RelayCommand(_ => ShowStockItemInventoryLog());
            RestockItemCommand = new Commands.RelayCommand(_ => ShowRestockItemDialog());
            EditProductCommand = new Commands.RelayCommand(_=> ShowEditProductDialog());
            DeleteProductCommand =new Commands.RelayCommand(_=> DiscontinueProduct() );

            _services = services;
            _dialogService = _services.GetRequiredService<IDialogService>();
            _transactionService = _services.GetRequiredService<ITransactionService>();
            _authTokenProvider = _services.GetRequiredService<IAuthTokenProvider>();
            _transactionService.SetAuthTokenProvider(_authTokenProvider);
            _internetConnectivityMonitorService = _services.GetRequiredService<InternetConnectivityMonitorService>();
            // subscribe to internet connectivity
            _internetConnectivityMonitorService.StatusChanged += OnInternetConnectivityStatusChanged;
            
            LoadTodaysOrders();
            LoadCurrentWeekOrders(); 
            LoadCurrentMonthOrders();
            

            Debug.WriteLine("Today's order: " + WeeklyOrders.Count);
            
            LoadDefaultStore();

            dateRangeHelper =new DateRangeHelper();
            DateRangeOptions = new ObservableCollection<DateRangeOption>(dateRangeHelper.GetDateRangeOptions());
            SelectedDateRange = DateRangeOptions.First();
            SelectedProductDateRange = DateRangeOptions.First();
        }
       

        public void SetLoggedInUserAccountType(int accountType)
        {
            LoggedInUserAccountType = accountType;
            if(accountType == 0)
            {
                OrderProcessedByAccountType = "Keeper";
            }
            else if (accountType == 1)
            {
                OrderProcessedByAccountType = "Merchant";
            }
            
            Debug.WriteLine($"Logged in user account type set to: {accountType}");
        }

        private void OnInternetConnectivityStatusChanged(ConnectivityStatus status)
        {
            Debug.WriteLine($"Internet connectivity status changed: {status.IsInternetConnected}");
            _isInternetConnected = status.IsInternetConnected;
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

        public string OrderProcessedByAccountType
        {
            get { return _orderProcessedByAccountType; }
            set
            {
                _orderProcessedByAccountType = value;
                OnPropertyChanged();
            }
        }
        
        public object? SelectedOrderControlTab
        {
            get { return _selectedOrderControlTab; }
            set
            {
                _selectedOrderControlTab = value;
                LoadSelectedTabControl(_selectedOrderControlTab);
                OnPropertyChanged();
            }
        }

        public object? CurrentOrderControlTab
        {
            get { return _currentOrderControlTab; }
            set
            {
                _currentOrderControlTab = value;
                OnPropertyChanged();
            }
        }
       
        public bool IsHeldOrdersBadgeVisible
        {
            get { return _isHeldOrdersBadgeVisible; }
            set
            {
                _isHeldOrdersBadgeVisible = value;
                OnPropertyChanged();
            }
        }

        public bool IsHeldOrdersHeaderVisible
        {
            get { return _isHeldOrdersHeaderVisible; }
            set
            {
                _isHeldOrdersHeaderVisible = value;
                OnPropertyChanged();
            }
        }

        public bool IsCheckoutButtonVisible
        {
            get { return _isCheckoutButtonVisible; }
            set
            {
                _isCheckoutButtonVisible = value;
                OnPropertyChanged();
            }
        }

        public bool IsInventoryTrackerDataGridVisible
        {
            get { return _isInventoryTrackerDataGridVisible; }
            set
            {
                _isInventoryTrackerDataGridVisible = value;
                OnPropertyChanged();
            }
        }

        public bool IsPendingOrdersDataGridVisible
        {
            get { return _isPendingOrdersDataGridVisible; }
            set
            {
                _isPendingOrdersDataGridVisible = value;
                OnPropertyChanged();
            }
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

        public ObservableCollection<Order> CustomerOrders
        {
            get { return _customerOrders; }
            set
            {
                _customerOrders = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Order> PendingOrders
        {
            get { return _pendingOrders; }
            set
            {
                _pendingOrders = value;
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

        public bool IsTopSellingCategoriesDataGridVisible
        {
            get { return _isTopSellingCategoriesDataGridVisible; }
            set
            {
                _isTopSellingCategoriesDataGridVisible= value;
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
                if (_selectedCategoryProducts.Count > 0)
                    IsCheckoutButtonEnabled = true;
                else 
                    IsCheckoutButtonEnabled = false;
               
                _selectedCategoryProducts.CollectionChanged += (s, e) => UpdateItemNumbers();
                UpdateItemNumbers();               
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ClosedOrder> ClosedOrders
        {
            get { return _closedOrders; }
            set
            {
                _closedOrders = value;
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

        public int SelectedAction
        {
            get { return _selectedAction; }
            set
            {
                _selectedAction = value;
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

        public decimal TotalDailySalesAmount
        {
            get { return _totalDailySalesAmount; }
            set
            {
                _totalDailySalesAmount = value;
                OnPropertyChanged();
            }
        }

        public decimal SelectedSalesAmount
        {
            get { return _selectedSalesAmount; }
            set
            {
                _selectedSalesAmount = value;
                OnPropertyChanged();
            }
        }

        public decimal TotalCashSalesAmount
        {
            get { return _totalCashSalesAmount; }
            set
            {
                _totalCashSalesAmount = value;
                OnPropertyChanged();
            }
        }

        public decimal TotalMobileMoneySalesAmount
        {
            get { return _totalMobileMoneySalesAmount; }
            set
            {
                _totalMobileMoneySalesAmount = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Order> TodaysOrders
        {
            get { return _todaysOrders; }
            set
            {
                _todaysOrders = value;
                TotalDailySalesAmount = _todaysOrders.Where(order => order.OrderStatus == 3)
                                                     .Sum(order => order.TotalAmountCents);
                ClosedOrdersCount = _todaysOrders.Count(order => order.OrderStatus == 3);
                OnPropertyChanged();
            }
        }


        public ObservableCollection<Order> WeeklyOrders
        {
            get { return _weeklyOrders; }
            set
            {
                _weeklyOrders = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Order> MonthlyOrders
        {
            get { return _monthlyOrders; }
            set
            {
                _monthlyOrders = value;
                OnPropertyChanged();
            }
        }

        public List<TopSellingCategoryProduct> TopSellingCategoryProducts
        {
            get { return _topSellingCategoryproducts; }
            set
            {
                _topSellingCategoryproducts = value;
                OnPropertyChanged();
            }
        }

        public List<TopSellingCategory> TopSellingCategories
        {
            get { return _topSellingCategories; }
            set
            {
                _topSellingCategories = value;
                OnPropertyChanged();
            }
        }

        public List<StockItem> StockItems
        {
            get { return _stockItems; }
            set
            {
                _stockItems = value;
                OnPropertyChanged();
            }
        }

        public string TopSellingItems
        {
            get { return _topSellingItems; }
            set
            {
                _topSellingItems = value;
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

        public int ClosedOrdersCount
        {
            get { return _closedOrdersCount; }
            set
            {
                _closedOrdersCount = value;
                OnPropertyChanged();
            }
        }

        public int OutOfStockCount
        {
            get { return _outOfStockCount; }
            set
            {
                _outOfStockCount = value;
                OnPropertyChanged();
            }
        }

        public int CriticalOnStockCount
        {
            get { return _criticalOnStockCount; }
            set
            {
                _criticalOnStockCount = value;
                OnPropertyChanged();
            }
        }

        public int LowOnStockCount
        {
            get { return _lowOnStockCount; }
            set
            {
                _lowOnStockCount = value;
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

        public string SelectedActionHeader
        {
            get { return _selectedActionHeader; }
            set
            {
                _selectedActionHeader = value;
                OnPropertyChanged();
            }
        }

        public string TotalSalesAmountHeader
        {
            get { return _totalSalesAmountHeader; }
            set
            {
                _totalSalesAmountHeader = value;
                OnPropertyChanged();
            }
        }

        public string SalesTypeHeader
        {
            get { return _salesTypeHeader; }
            set
            {
                _salesTypeHeader = value;
                OnPropertyChanged();
            }
        }

        public decimal TotalSalesAmountValue
        {
            get { return _totalSalesAmountValue; }
            set
            {
                _totalSalesAmountValue = value;
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

        public bool IsSalesDataGridVisible
        {
            get { return _isSalesDataGridVisible; }
            set
            {
                _isSalesDataGridVisible = value;
                OnPropertyChanged();
            }
        }

        public bool IsOrdersDataGridVisible
        {
            get { return _isOrdersDataGridVisible; }
            set
            {
                _isOrdersDataGridVisible = value;
                OnPropertyChanged();
            }
        }

        public bool IsTopSellingItemsDataGridVisible
        {
            get { return _isTopSellingItemsDataGridVisible; }
            set
            {
                _isTopSellingItemsDataGridVisible = value;
                OnPropertyChanged();
            }
        }

        public bool IsHeldOrdersDataGridVisible
        {
            get {return _isHeldOrdersDataGridVisible;}
            set
            {
                _isHeldOrdersDataGridVisible = value;
                OnPropertyChanged();
            }
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

        public bool IsCheckoutButtonEnabled
        {
            get { return _isCheckoutButtonEnabled; }
            set
            {
                _isCheckoutButtonEnabled = value;
                OnPropertyChanged();
            }
        }

        public int PausedOrdersCount
        {
            get { return _pausedOrdersCount; }
            set
            {
                _pausedOrdersCount = value;
                OnPropertyChanged();
            }
        }

        public Order CurrentOrder
        {
            get { return _currentOrder; }
            set
            {
                _currentOrder = value;
                OnPropertyChanged();
            }
        }

        public string? FriendlyOrderItemsList
        {
            get { return _friendlyOrderItemsList; }
            set
            {
                _friendlyOrderItemsList = value;
                OnPropertyChanged();
            }
        }

        public string? HeldOrdersTitle
        {
            get { return _heldOrdersTitle; }
            set
            {
                _heldOrdersTitle = value;
                OnPropertyChanged();
            }
        }

        public string? FriendlyClosedOrdersItemsList
        {
            get { return _friendlyClosedOrdersItemsList; }
            set
            {
                _friendlyClosedOrdersItemsList = value;
                OnPropertyChanged();
            }
        }

        public string? ClosedOrdersTitle
        {
            get { return _closedOrdersTitle; }
            set
            {
                _closedOrdersTitle = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Sale> Sales
        {
            get { return _sales; }
            set
            {
                _sales = value;
                OnPropertyChanged();
            }
        }

        public DateRangeOption SelectedDateRange
        {
            get { return _selectedDateRange; }
            set
            {
                _selectedDateRange = value;
                Debug.WriteLine($"Selected date range: {value.GroupByScope} from {value.StartDate.ToShortDateString()} to {value.EndDate.ToShortDateString()}");
                SalesTypeHeader = $"Top Selling Categories - ({value.DisplayName})" ?? "Top Selling Categories";
                TotalSalesAmountHeader = $"Total Sales Amount" ?? "Total Sales Amount";
                using var _ = ShowTopSellingCategoriesByDateRange();
                OnPropertyChanged();
            }
        }

        public DateRangeOption SelectedProductDateRange
        {
            get { return _selectedProductDateRange; }
            set
            {
                _selectedProductDateRange = value;
                Debug.WriteLine($"Selected product date range: {value.GroupByScope} from" +
                    $" {value.StartDate.ToShortDateString()} to {value.EndDate.ToShortDateString()}");
                SalesTypeHeader = $"Top Selling Products - ({value.DisplayName})" ?? "Top Selling Products";
                
                TotalSalesAmountHeader = $"Total Sales Amount" ?? "Total Sales Amount";
                using var _ = ShowTopSellingProductsByDateRange();
                OnPropertyChanged();
            }
        }

        public StockItem SelectedStockItem
        {
            get { return _selectedStockItem; }
            set
            {
                _selectedStockItem = value;
                OnPropertyChanged();
            }
        }


        private void _selectedCategoryProducts_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public CategoryProduct? SelectedCategoryProduct
        {
            get { return _selectedCategoryProduct; }
            set
            {
                _selectedCategoryProduct = value;
                if (_selectedCategoryProduct != null) // Ensure the value is not null before calling the method
                {
                    if (CurrentOrder != null)
                        CurrentOrder.OrderProductItems.Add(_selectedCategoryProduct);

                    AddOrUpdateSelectedCategoryProducts(_selectedCategoryProduct);
                    IsUserQuerying = false;
                    SearchQuery = string.Empty;
                    IsSelectedCategoryProductsGridVisible = true;
                    IsPauseOrderButtonVisible = true;
                }
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
        
        public PausedOrder SelectedPausedOrder
        {
            get { return _selectedPausedOrder; }
            set
            {
                _selectedPausedOrder = value;
                if (_selectedPausedOrder != null)
                {
                    PausedOrder = _selectedPausedOrder.SavedOrder;
                    HeldOrdersTitle = $"The items below are in Order No. {PausedOrder.Id}'s shopping basket.";     
                    TotalAmount = PausedOrder.TotalAmountCents;
                    FriendlyOrderItemsList = ListFormatterHelper.ToHyphenedString(_selectedPausedOrder.SavedOrder.OrderProductItems, item => item.ProductName);
                    UpdateOrderItemNumbers();
                }
                OnPropertyChanged();
            }
        }

        public ClosedOrder SelectedClosedOrder
        {
            get { return _selectedClosedOrder; }
            set
            {
                _selectedClosedOrder = value;
                if (_selectedClosedOrder != null)
                {
                    ClosedOrdersTitle = $"The items below were in Order No. {_selectedClosedOrder.SavedOrder.Id}'s shopping basket.";
                    FriendlyClosedOrdersItemsList = ListFormatterHelper.ToHyphenedString(_selectedClosedOrder.SavedOrder.OrderProductItems, item => item.ProductName);
                    UpdateOrderItemNumbers();
                }
                OnPropertyChanged();
            }
        }

        public ObservableCollection<PausedOrder> PausedOrders
        {
            get { return _pausedOrders; }
            set
            {
                _pausedOrders = value;
                OnPropertyChanged();
            }
        }

        public Order PausedOrder
        {
            get { return _pausedOrder; }
            set
            {
                _pausedOrder=value;
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

        public ICommand PauseCurrentOrderCommand { get; }

        public ICommand CancelOrderCommand { get; }
        public ICommand ResumeOrderCommand { get; }
        public ICommand ShowTodaysSalesCommand { get; }
        public ICommand ShowThisWeeksSalesCommand { get; }

        public ICommand ShowThisMonthsSalesCommand { get; }

        public ICommand GenerateChartCommand { get; }
        public ICommand ShowClosedOrdersCommand { get; }
        public ICommand ShowHeldOrdersCommand { get; }
        
        public ICommand ShowPaymentsCommand { get; }
        public ICommand ShowPaymentModesChartCommand { get; }
        public ICommand ShowTopSellingProductsCommand { get; }
        public ICommand ShowTopCategoriesCommand { get; }
        public ICommand ShowInventoryTrackerCommand { get; }

        public ICommand StockActionCommand { get; }

        public ICommand InventoryLogCommand { get; }
        public ICommand RestockItemCommand { get; }

        public ICommand EditProductCommand { get; }

        public ICommand DeleteProductCommand { get; }

        async void LoadInventoryCategoryProducts()
        {
            _inventoryCategoryProducts = await dataObjects.GetCategoryProductsAsync();            
        }
        async void LoadLatestCustomerOrders()
        {       
            var orders = await dataObjects.ShopOrders.SortOrderByDateAndOrderStatus(DateTime.Now, CustomerOrderStatus.Closed, SortDirection.Descending);
            _customerOrders.Clear();
            foreach (var order in orders)
            {
                if (order.OrderStatus == 3)
                {
                    order.OrderProcessedByUserName = _userLookupCache.TryGetValue(order.OrderProcessedById, out var userName) ? userName : "Unknown";
                    _customerOrders.Add(order);
                }
            }                  
            CustomerOrdersCount = _customerOrders.Count + 1;
        }

        async void LoadTodaysOrders()
        {
            var orders = await dataObjects.GetTodaysOrdersAsync();
            _todaysOrders.Clear();
            foreach (var order in orders)
            {
                _todaysOrders.Add(order);
            }
            TotalDailySalesAmount = _todaysOrders.Where(order => order.OrderStatus == 3)
                                                 .Sum(order => order.TotalAmountCents);
            ClosedOrdersCount = _todaysOrders.Count(order => order.OrderStatus == 3);
            
        }

        async void LoadCurrentWeekOrders()
        {
            var orders = await dataObjects.GetCurrentWeekEntitiesAsync();
            _weeklyOrders.Clear();
            foreach (var order in orders)
            {
                _weeklyOrders.Add(order);
            }            
        }

        async void LoadCurrentMonthOrders()
        {
            var orders = await dataObjects.GetCurrentMonthEntitiesAsync();
            _monthlyOrders.Clear();
            foreach (var order in orders)
            {
                _monthlyOrders.Add(order);
            }
            
        }

        async void LoadPendingOrders()
        {
            var orders = await dataObjects.GetCurrentDayPendingOrdersAsync();
            _pendingOrders.Clear();
            foreach (var order in orders)
            {
                order.OrderProcessedByUserName = _userLookupCache.TryGetValue(order.OrderProcessedById, out var userName) ? userName : "Unknown";
                PausedOrder pausedOrder =new PausedOrder(order);
                _pausedOrders.Add(pausedOrder);
                _pendingOrders.Add(order);
            }      
            OnPropertyChanged(nameof(PausedOrders));
        }

        async void LoadTopSellingProducts()
        {
            _topSellingCategoryproducts = await dataObjects.GetTopSellingCategoryProductsAsync();
            TopSellingItems = ListFormatterHelper.ToHyphenedString(_topSellingCategoryproducts.Take(3), item => item.ProductName);
            TopSellingCategoryProducts = _topSellingCategoryproducts;            
        }

        async void LoadTopSellingCategories()
        {
            _topSellingCategories = await dataObjects.GetTopSellingCategoriesAsync();
            TopSellingItems = ListFormatterHelper.ToHyphenedString(_topSellingCategories.Take(3), item => item.CategoryName);
            TopSellingCategories = _topSellingCategories;

        }

        async void LoadDefaultStore()
        {
            CurrentStore = await dataObjects.GetDefaultStoreAsync();           
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

        private async Task DiscontinueProduct()
        {
            if (SelectedStockItem != null)
            {
                var defaultUserAccount = ApplicationState.Instance.DefaultAccount;
                if (defaultUserAccount != null)
                {
                    var categoryProduct = await dataObjects.CategoryProducts.GetById(SelectedStockItem.Id);
                    if (categoryProduct != null)
                    {
                        var result =
                            MessageBox.Show($"This product will be deleted from your inventory. " +
                            $"Are you sure you want to discontinue this product?", "Delete Product?", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (result == MessageBoxResult.Yes)
                        {
                            bool deleted = await dataObjects.CategoryProducts.DeleteWithStatusAsync(categoryProduct.Id);
                            if (deleted)
                            {                                
                                var lastInventoryLog = await dataObjects.GetLastInventoryTrackerForProduct(categoryProduct.Id);
                                if (lastInventoryLog != null)
                                {
                                    var inventoryTracker = new InventoryTracker
                                    {
                                        CategoryProductId = categoryProduct.Id,                                        
                                        InventoryId = lastInventoryLog.InventoryId,
                                        ActionTaken = 4, // item deleted/discontinued
                                        OldQuantity = lastInventoryLog.OldQuantity,
                                        NewQuantity = categoryProduct.Units,
                                        UserAccountId = (int)defaultUserAccount.OnlineUserAccountId,
                                        QuantityAction = -categoryProduct.Units,
                                        ReferenceId = $"REF-REM-{categoryProduct.Id}-{DateTime.Now:yyyyMMddHHmmss}",
                                        SalePrice = 0,
                                        PurchasePrice = 0,
                                        Remarks = $"{categoryProduct.ProductName} deleted and discontinued from inventory",
                                        CreatedAt = DateTime.Now,
                                    };
                                    await dataObjects.TrackInventoryChange(inventoryTracker);
                                }

                                var currentStore = ApplicationState.Instance.CurrentStore;
                                string accountType = defaultUserAccount.AccountType == 1 ? "merchant" : "keeper";
                                var unSyncedObject = new UnSyncedObject
                                {
                                    OnlineStoreId = currentStore.OnlineStoreId,
                                    onlineUnSyncedObjectId = categoryProduct.OnlineCategoryProductId,
                                    objectId = categoryProduct.Id,
                                    objectTableName = "CategoryProducts",
                                    actionTaken = 3,
                                    fromFullJid = defaultUserAccount.FullJid,
                                    toFullJid = defaultUserAccount.BareJid,
                                    AuthToken = defaultUserAccount.AuthToken,
                                    AccountType = accountType,
                                    CategorySectionId = categoryProduct.CategorySectionId
                                };
                                var savedSyncedObject = await dataObjects.UnSyncedObjects.SaveAndReturnEntityAsync(unSyncedObject);
                                if (savedSyncedObject != null)
                                {
                                    using var pipe = new NamedPipeClientStream(
                                        ".",
                                        "ChoziShopUnSyncedObjectsPipe",
                                        PipeDirection.Out);
                                    try
                                    {
                                        await pipe.ConnectAsync(5000);
                                        var json = JsonConvert.SerializeObject(savedSyncedObject);
                                        var bytes = Encoding.UTF8.GetBytes(json);
                                        await pipe.WriteAsync(bytes, 0, bytes.Length);
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.WriteLine("connection to pipe failed: ", ex.Message);
                                    }
                                }
                                StockItems.Remove(SelectedStockItem);
                                MessageBox.Show("Product successfully removed from inventory.", "Product Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Product deletion failed. You need administrative previleges to perform action.",
                        "Login Required!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ShowSelectedStockAction(object? param)
        {
            if (param is string stockAction)
            {
                switch (stockAction)
                {
                    case "OutOfStock":
                        StockItems = dataObjects.GroupStockItemsByStatusAsync(_inventoryCategoryProducts, ProductInventoryStatus.Empty);
                        UpdateStockItemNumbers();
                        SelectedActionHeader = $"{StockItems.Count} Out of Stock Items";
                        OutOfStockCount = StockItems.Count;
                        break;
                    case "Critical":
                        StockItems = dataObjects.GroupStockItemsByStatusAsync(_inventoryCategoryProducts, ProductInventoryStatus.Critical);
                        UpdateStockItemNumbers();
                        SelectedActionHeader = $"{StockItems.Count} Critical on Stock Items";
                        CriticalOnStockCount = StockItems.Count;
                        break;
                    case "Low":
                        StockItems = dataObjects.GroupStockItemsByStatusAsync(_inventoryCategoryProducts, ProductInventoryStatus.Low);
                        UpdateStockItemNumbers();
                        SelectedActionHeader = $"{StockItems.Count} Low on Stock Items";
                        LowOnStockCount = StockItems.Count;
                        break;
                    default:
                        Debug.WriteLine("Unknown stock action selected.");
                        break;
                }
            }
        }
        private void LoadSelectedTabControl(object? selectedOrderControlTab)
        {
            if (selectedOrderControlTab is TabItem tabItem)

            {
                switch (tabItem.Name)
                {
                    case "HeldOrders":
                        Debug.WriteLine("Show HeldOrdersControl");                                                
                        CurrentOrderControlTab = _services.GetRequiredService<HeldOrdersControl>();
                        ShowPausedOrders();
                        tabItem.IsSelected = true;
                        IsHeldOrdersDataGridVisible = true;
                        IsPauseOrderButtonVisible = false;
                        IsCheckoutButtonVisible = false; // Hide checkout button in HeldOrders tab
                        break;
                    case "OpenOrders":
                        Debug.WriteLine("Show OpenOrders control");
                        CurrentOrderControlTab = _services.GetRequiredService<OpenOrdersControl>();
                        tabItem.IsSelected = true;
                        IsPauseOrderButtonVisible = false;
                        IsCheckoutButtonVisible = false;
                        break;
                    case "ClosedOrders":
                        Debug.WriteLine("Show ClosedOrders control");
                        CurrentOrderControlTab = _services.GetRequiredService<ClosedOrdersControl>();
                        tabItem.IsSelected = true;
                        IsPauseOrderButtonVisible = false;
                        IsCheckoutButtonVisible = false; // Hide checkout button in ClosedOrders tab
                        break;
                    default:
                        CurrentOrderControlTab = _services.GetRequiredService<CurrentOrderControl>();
                        tabItem.IsSelected = true;
                        _currentOrderTabItem = tabItem;
                        IsPauseOrderButtonVisible = true;
                        IsCheckoutButtonVisible = true;
                        Debug.WriteLine("CurrentOrders Control");
                        break;
                }
            }
            else
            {
                if (_currentOrderTabItem != null)
                {

                    _currentOrderTabItem.IsSelected = true;
                }
            }
        }

        private void AddOrUpdateSelectedCategoryProducts(CategoryProduct categoryProduct)
        {
            if (categoryProduct == null) return;

            if (categoryProduct.Units <= 0)
            {
                MessageBox.Show("Cannot add selected item to order. Product is out of stock.", "Out of Stock", MessageBoxButton.OK, MessageBoxImage.Warning);
                Debug.WriteLine("Cannot add product to order. Product is out of stock.");
                return; // Do not allow adding products that are out of stock
            }

            var existing = SelectedCategoryProducts.FirstOrDefault(product => product.CategoryProduct?.Id == categoryProduct.Id);
            if (existing != null)
            {
                int availableUnitsCount = existing.CategoryProduct.Units--;
                if (availableUnitsCount <= 0)
                {
                    MessageBox.Show("Cannot add selected item to order. Product is out of stock.", "Out of Stock", MessageBoxButton.OK, MessageBoxImage.Warning);
                    Debug.WriteLine("Cannot add product to order. Product is out of stock.");
                    return; // Do not allow adding products that are out of stock
                }

                existing.UnitCount++;
                existing.TotalPrice = existing.UnitCount * existing.UnitPrice;
                var existingTracker = _inventoryTrackers.FirstOrDefault(x => x.CategoryProductId == categoryProduct.Id);
                if (existingTracker != null)
                {
                    // Update quantity action to reflect the number of items sold
                    existingTracker.QuantityAction = -existing.UnitCount;
                    // Update the new quantity in the tracker
                    existingTracker.NewQuantity = existingTracker.NewQuantity - 1;
                    existingTracker.Remarks = "Sold " + existing.UnitCount + " units of " + categoryProduct.ProductName;
                    Debug.WriteLine($"Inventory tracker update status: OldQuantity: ${existingTracker.OldQuantity} - NewQuantity: {existingTracker.NewQuantity} " +
                        $"- QuantityAction: {existingTracker.QuantityAction} - Remarks: {existingTracker.Remarks}");
                }
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
                int deductionCount = SelectedCategoryProducts.Where(x => x.CategoryProduct.Id == categoryProduct.Id).Count();
                InventoryTracker inventoryTracker = new InventoryTracker
                {
                    CategoryProductId = categoryProduct.Id,
                    ActionTaken = 1, // Item is being sold
                    OldQuantity = categoryProduct.Units,
                    NewQuantity = categoryProduct.Units - 1,
                    QuantityAction = -deductionCount,
                    SalePrice = categoryProduct.UnitCost,
                    Remarks = "Sold " + deductionCount + " units of " + categoryProduct.ProductName
                };
                _inventoryTrackers.Add(inventoryTracker);
                Debug.WriteLine($"Inventory tracker status: OldQuantity: ${inventoryTracker.OldQuantity} - NewQuantity: {inventoryTracker.NewQuantity} " +
                    $"- QuantityAction: {inventoryTracker.QuantityAction}");

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
                    if (!_lowOnStockCategoryProducts.Contains(categoryProduct))
                    {
                        _lowOnStockCategoryProducts.Add(categoryProduct);
                    }
                    LowOnStockHeader = GetStockStatusHeader(_lowOnStockCategoryProducts, 2);
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
            categoryProduct.Units--;
        }

        private void RemoveSelectedProductItem(SelectedCategoryProduct selectedProductItem)
        {
            Debug.WriteLine("Removing selected product item: " + selectedProductItem?.ItemName);
            if (selectedProductItem == null) return;
            if (CurrentOrder != null)
            {
                // Remove product from current order too
                var productToRemove = CurrentOrder.OrderProductItems.FirstOrDefault(p => p.OnlineCategoryProductId == selectedProductItem.CategoryProduct.OnlineCategoryProductId);
                if (productToRemove != null)
                    CurrentOrder.OrderProductItems.Remove(productToRemove);
            }
            var existing = SelectedCategoryProducts.FirstOrDefault(product => product.CategoryProduct.OnlineCategoryProductId == selectedProductItem.CategoryProduct.OnlineCategoryProductId);
            if (existing != null)
            {
                if (SelectedProductItem.UnitCount > 1)
                {
                    Debug.WriteLine("Decreasing unit count for existing item: " + existing.ItemName);
                    existing.UnitCount--;
                    existing.TotalPrice = existing.UnitCount * existing.UnitPrice;
                    NoOfItems = SelectedCategoryProducts.Sum(x => x.UnitCount);

                    var existingTracker = _inventoryTrackers.FirstOrDefault(x => x.CategoryProductId == selectedProductItem.CategoryProduct.Id);
                    if (existingTracker != null)
                    {
                        // Update quantity action to reflect the number of items sold by decreasing the count
                        existingTracker.QuantityAction = -existing.UnitCount;
                        // Update the new quantity in the tracker by increasing it by one 
                        existingTracker.NewQuantity = existingTracker.NewQuantity + 1;
                        Debug.WriteLine($"Inventory tracker update status: OldQuantity: ${existingTracker.OldQuantity} - NewQuantity: {existingTracker.NewQuantity} " +
                     $"- QuantityAction: {existingTracker.QuantityAction} - Remarks: {existingTracker.Remarks}");
                    }

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
                    var existingTracker = _inventoryTrackers.FirstOrDefault(x => x.CategoryProductId == selectedProductItem.CategoryProduct.Id);
                    if(existingTracker != null)
                    {
                        // Remove tracker since product has been removed too
                        _inventoryTrackers.Remove(existingTracker);

                    }
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
                int availableUnitsCount = existing.CategoryProduct.Units--;
                if (availableUnitsCount <= 0)
                {
                    MessageBox.Show("Cannot add selected item to order. Product is out of stock.", "Out of Stock", MessageBoxButton.OK, MessageBoxImage.Warning);
                    Debug.WriteLine("Cannot add product to order. Product is out of stock.");
                    return; // Do not allow adding products that are out of stock
                }

                existing.UnitCount++;
                existing.TotalPrice = existing.UnitCount * existing.UnitPrice;
                TotalAmount = SelectedCategoryProducts.Sum(x => x.TotalPrice);
                NoOfItems = SelectedCategoryProducts.Sum(x => x.UnitCount);

                var existingTracker = _inventoryTrackers.FirstOrDefault(x => x.CategoryProductId == selectedProductItem.CategoryProduct.Id);
                if (existingTracker != null)
                {
                    existingTracker.QuantityAction = -existing.UnitCount;
                    existingTracker.NewQuantity = existingTracker.NewQuantity - 1;
                    existingTracker.Remarks = "Sold " + existing.UnitCount + " units of " + existing.CategoryProduct.ProductName;
                    Debug.WriteLine($"Inventory tracker update status: OldQuantity: ${existingTracker.OldQuantity} - NewQuantity: {existingTracker.NewQuantity} " +
                     $"- QuantityAction: {existingTracker.QuantityAction} - Remarks: {existingTracker.Remarks}");
                }

                
                var inventoryStatus = InventoryStatus.GetCategoryProductInventoryStatus(existing.CategoryProduct.Units);
                Debug.WriteLine("Inventory status: " + inventoryStatus + " No of units: " + existing.CategoryProduct.Units);
                Debug.WriteLine("Existing is " + existing.ItemName + " with unit count: " + existing.UnitCount);

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
                        // Remove the product from the low on stock list if it exists
                        if (_lowOnStockCategoryProducts.Contains(existing.CategoryProduct))
                        {
                            _lowOnStockCategoryProducts.Remove(existing.CategoryProduct);
                            LowOnStockHeader = GetStockStatusHeader(_lowOnStockCategoryProducts, 2);
                            IsLowOnStockHeaderVisible = false;
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
        }

        private void ResumeOrder()
        {
            if (CurrentOrder == null)
            {
                IsSelectedCategoryProductsGridVisible = true;
                SelectedOrderControlTab = _services.GetRequiredService<CurrentOrderControl>();
                CurrentOrder = SelectedPausedOrder.SavedOrder;
                NoOfItems = SelectedPausedOrder.SavedOrder.OrderProductItems.Count;
                TotalAmount = SelectedPausedOrder.SavedOrder.OrderProductItems.Sum(x => x.UnitCost);
                foreach (CategoryProduct cp in SelectedPausedOrder.SavedOrder.OrderProductItems)
                {
                    AddOrUpdateSelectedCategoryProducts(cp);
                }
                PausedOrders.Remove(SelectedPausedOrder);
                if (PausedOrders.Count > 0)
                {
                    SelectedPausedOrder = PausedOrders[0];
                }
                else
                {
                    SelectedPausedOrder = null;
                    HeldOrdersTitle = string.Empty;
                    FriendlyOrderItemsList = string.Empty;
                }
            }
            else
            {
                MessageBox.Show(
                    "You already have a previously held order in progress. Please complete it before resuming another order.",
                    "Held Order In Progress", MessageBoxButton.OK, MessageBoxImage.Warning
                    );
            }
        }

        private async Task CancelOrder()
        {
            if (SelectedPausedOrder == null)
            {
                MessageBox.Show("Please select an order to cancel.", "No Order Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            else
            {
                var result = MessageBox.Show($"Are you sure you want to cancel this order?", "Cancel Order No. {order.id}?", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    if (SelectedPausedOrder.SavedOrder != null)
                    {
                        var order = SelectedPausedOrder.SavedOrder;
                        bool deleted = await dataObjects.ShopOrders.DeleteWithStatusAsync(order.Id);
                        if (deleted)
                        {
                            MessageBox.Show("Order has been removed. Order was canceled and removed from pending orders.",
                                "Order Discontinued", MessageBoxButton.OK, MessageBoxImage.Hand);
                        }
                        PendingOrders.Remove(order);
                    }
                    // Cancel the order and remove it from the list
                    PausedOrders.Remove(SelectedPausedOrder);
                    
                   
                }
            }
        }

       private void UpdateTopSellingProductNumbers()
        {
            if (TopSellingCategoryProducts.Count > 0)
            {
                for (int i = 0; i < TopSellingCategoryProducts.Count; i++)
                {
                    TopSellingCategoryProducts[i].ItemNo = i + 1;
                }
            }
        }

        private void UpdateItemNumbers()
        {            
            for (int i = 0; i < SelectedCategoryProducts.Count; i++)
            {
                Debug.WriteLine(SelectedCategoryProducts[i].CategoryProduct.ProductName + " " + SelectedCategoryProducts[i].UnitCount);
                SelectedCategoryProducts[i].ItemNo = i + 1;
            }
        }

        private void UpdateStockItemNumbers()
        {
            if (StockItems.Count > 0)
            {
                for (int i = 0; i < StockItems.Count; i++)
                {
                    StockItems[i].ItemNo = i + 1;
                }
            }
        }

        private void UpdateOrderItemNumbers()
        {
            if (PausedOrders.Count > 0)
            {
                for (int i = 0; i < PausedOrders.Count; i++)
                {
                    PausedOrders[i].ItemNo = i + 1;
                }
            }

            if (ClosedOrders.Count > 0)
            {
                for (int i = 0; i < ClosedOrders.Count; i++)
                {
                    ClosedOrders[i].ItemNo = i + 1;
                }
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
            if (_isInternetConnected)
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
            else
            {
                MessageBox.Show("An internet connection is required to process mobile money payments. Please check your connection and try again.", "No Internet Connection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async Task<Order?> ProcessCustomerOrder()
        {
            IsNoNewActionPossible = false;
            var _defaultUserAccount = ApplicationState.Instance.DefaultAccount;
            if (_defaultUserAccount != null)
            {
                var order = new Order()
                {
                    StoreId = 171,
                    PreferredPaymentMode = 1,
                    PaymentStatus = 0,
                    OrderStatus = 2,
                    TotalAmountCents = TotalAmount,
                    TotalAmountCurrency = "UGX",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    OfflineUpdatedAt = DateTime.Now,
                    OrderProcessedById = _defaultUserAccount.OnlineUserAccountId,
                    OrderProductItems = SelectedCategoryProducts.Select(x => x.CategoryProduct).ToList()
                };

                IsLoadingLineVisible = true;
                IsMobilePaymentStatusVisible = true;
                _baseApi = new BaseApi(_defaultUserAccount.AuthToken);
                // start processing order 
                var serializedOrder = await _baseApi.ProcessMobileMoneyOrder(order, "Merchant");
                if (serializedOrder != null)
                {
                    Debug.WriteLine("Order successfully saved to API with OnlineId: " + serializedOrder.Id);
                    // save order to db
                    order.OnlineOrderId = serializedOrder.Id;
                    order.OrderProcessedById = _defaultUserAccount.OnlineUserAccountId;

                    var savedOrder = await dataObjects.ShopOrders.SaveAndReturnEntityAsync(order);

                    return savedOrder;
                }
            }
            else
            {
                MessageBox.Show("Your session might have expired. Make sure you have authorization to process customer orders.", "Action Requires Authorization!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return null;
        }

        private async void UpdateCategoryProductCount()
        {
            if (SelectedCategoryProducts.Count > 0)
            {
                for (int i = 0; i < SelectedCategoryProducts.Count; i++)
                {
                    var categoryProduct = SelectedCategoryProducts[i].CategoryProduct;
                    if (categoryProduct != null)
                    {
                        categoryProduct.UpdatedAt = DateTime.Now;
                        await dataObjects.CategoryProducts.UpdateAsync(categoryProduct);
                        Debug.WriteLine($"Updated {categoryProduct.ProductName} units to {categoryProduct.Units}");
                    }
                }
            }
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
                    await StartStatusPolling(savedAirtelPayCollection, order);
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

        private async Task StartStatusPolling(AirtelPayCollection airtelPayCollection, Order order)
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
                              await  transactionSuccessful(order);
                                break;
                            case TransactionStatus.TF:
                                transactionFailed(order);
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

        private async Task<Order?> PauseCurrentOrder()
        {
            if (CurrentStore != null)
            {
                if (CurrentOrder != null && CurrentOrder.OrderStatus == 1)
                {                                        
                    CurrentOrder.TotalAmountCents = CurrentOrder.OrderProductItems.Sum(x => x.UnitCost);                   
                    PausedOrder pausedOrder = new PausedOrder(CurrentOrder);
                    PausedOrders.Add(pausedOrder);
                    SelectedPausedOrder = PausedOrders[0];
                    UpdateOrderItemNumbers();
                    PausedOrdersCount = PausedOrders.Count;
                    IsHeldOrdersBadgeVisible = true;
                    IsHeldOrdersHeaderVisible = false;
                    MessageBox.Show("Order has been paused again.", "Order Held!", MessageBoxButton.OK, MessageBoxImage.Information);
                    CurrentOrder = null;
                    SelectedCategoryProducts.Clear();
                    TotalAmount = 0;
                    NoOfItems = 0;
                    SelectedCategoryProduct = null;
                    return pausedOrder.SavedOrder;
                }
                else if(SelectedCategoryProducts!= null && SelectedCategoryProducts.Count > 0 && CurrentOrder == null)
                {
                    IsNoNewActionPossible = true;                    
                    var order = new Order
                    {
                        StoreId = CurrentStore.Id,
                        PreferredPaymentMode = 0,
                        PaymentStatus = 0,
                        OrderStatus = 1,
                        TotalAmountCents = TotalAmount,
                        TotalAmountCurrency = "UGX",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        OfflineUpdatedAt = DateTime.Now,
                        OrderProcessedById = ApplicationState.Instance.DefaultAccount.OnlineUserAccountId,
                        OrderProductItems = SelectedCategoryProducts.Select(x => x.CategoryProduct).ToList()
                    };
                    // Save order to database
                    var savedOrder = await dataObjects.ShopOrders.SaveAndReturnEntityAsync(order);
                    if (savedOrder != null && savedOrder.Id > 0)
                    {
                        PendingOrders.Add(savedOrder);
                        savedOrder.OrderProcessedByUserName = ApplicationState.Instance.DefaultAccount?.FullName ?? "Unknown User";
                        PausedOrder pausedOrder = new PausedOrder(savedOrder);
                        PausedOrders.Add(pausedOrder);
                        SelectedPausedOrder = PausedOrders[0];
                        UpdateOrderItemNumbers();
                        PausedOrdersCount = PausedOrders.Count;
                        IsHeldOrdersBadgeVisible = true;
                        IsHeldOrdersHeaderVisible = false;
                        SelectedCategoryProducts.Clear();
                        TotalAmount = 0;
                        NoOfItems = 0;
                        SelectedCategoryProduct = null;
                        MessageBox.Show("Order successfully saved and status changed to held state.", "Order Held!", MessageBoxButton.OK, MessageBoxImage.Information);
                        return savedOrder;
                    }

                }
                else
                {
                    MessageBox.Show("You cannot pause an empty order. Please add items first.", "Order Not Saved!", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            return null;
        }

        private async Task transactionSuccessful(Order order)
        {
            if (_cancellationTokenSource != null)
            {
                var currentUser = ApplicationState.Instance.DefaultAccount;
                var merchantAccount = _authTokenProvider.GetCurrentMerchantAccount();
                order.PaymentStatus = 1; // paid
                order.OrderStatus = 3; // closed
                // When using mobile money, only update when transaction has completed successfully
                var updatedOrder = await dataObjects.ShopOrders.UpdateAndReturnEntityAsync(order);
                if (updatedOrder != null)
                {
                    updatedOrder.OrderProcessedByUserName = ApplicationState.Instance.DefaultAccount?.FullName ?? "Unknown User";
                    CustomerOrders.Add(updatedOrder);
                    TodaysOrders.Add(updatedOrder);
                    TotalDailySalesAmount = TodaysOrders.Where(o => o.OrderStatus == 3)
                        .Sum(x => x.TotalAmountCents);
                    Debug.WriteLine("Order successfully updated with order status: " + order.OrderStatus);

                    if (merchantAccount != null)
                    {
                        var unSyncedObject = new UnSyncedObject
                        {
                            OnlineStoreId = order.StoreId,
                            onlineUnSyncedObjectId = order.OnlineOrderId,
                            objectId = order.Id,
                            objectTableName = "Orders",
                            syncStatus = 1,
                            actionTaken = 0,
                            fromFullJid = currentUser.FullJid,
                            toFullJid = merchantAccount.FullJid,
                            AuthToken = currentUser.AuthToken,
                            AccountType = currentUser.DefaultAccountType
                        };
                        var savedUnSyncedObject = await dataObjects.UnSyncedObjects.SaveAndReturnEntityAsync(unSyncedObject);
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
                                Debug.WriteLine($"Error sending un-synced object ChoziShopUnSyncedObjectsPipe: {ex.Message}");
                            }
                        }
                    }
                }

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

        private void transactionFailed(Order order)
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

        private async Task ProcessCashOrder()
        {
            if (CurrentOrder != null && CurrentOrder.OrderStatus == 1)
            {
                if (_isValidCheckout)
                {
                    MessageBox.Show("This is a current order, meaning it's already been saved. We therefore update it with whatever changes.", "Order Paused!");
                    return;
                }
            }
            else
            {
                if (_isValidCheckout)
                {
                    if (CurrentStore != null)
                    {
                        var _defaultUserAccount = ApplicationState.Instance.DefaultAccount;
                        if (_defaultUserAccount != null)
                        {
                            var order = new Order
                            {
                                StoreId = CurrentStore.Id,
                                PreferredPaymentMode = 0, // Cash
                                PaymentStatus = 1, // paid
                                OrderStatus = 3, // closed
                                TotalAmountCents = TotalAmount,
                                TotalAmountCurrency = "UGX",
                                CreatedAt = DateTime.Now,
                                UpdatedAt = DateTime.Now,
                                OfflineUpdatedAt = DateTime.Now,
                                OrderProcessedById = _defaultUserAccount.OnlineUserAccountId, // TODO: Get current user id
                                OrderProductItems = SelectedCategoryProducts.Select(x => x.CategoryProduct).ToList()
                            };
                            var savedOrder = dataObjects.ShopOrders.SaveAndReturnEntityAsync(order).Result;
                            if (savedOrder.Id > 0)
                            {
                              //  Application.Current.Dispatcher.Invoke(() =>
                              //  {
                                    savedOrder.OrderProcessedByUserName = _defaultUserAccount.FullName;
                                    CustomerOrders.Add(savedOrder);
                                    TodaysOrders.Add(savedOrder);
                                    TotalDailySalesAmount = TodaysOrders.Where(o => o.OrderStatus == 3)
                                        .Sum(x => x.TotalAmountCents);
                                    ClosedOrdersCount = TodaysOrders.Where(o => o.OrderStatus == 3).Count();
                              //  });


                                UpdateCategoryProductCount();
                                foreach (InventoryTracker tracker in _inventoryTrackers)
                                {
                                    tracker.InventoryId = CurrentStore.InventoryId;
                                    tracker.UserAccountId = (int)_defaultUserAccount.OnlineUserAccountId;
                                    tracker.CreatedAt = savedOrder.CreatedAt;
                                    tracker.ReferenceId = $"REF-ORDER-NO-{savedOrder.Id}";
                                    await dataObjects.TrackInventoryChange(tracker);
                                    Debug.WriteLine($"TotalSalePrice: {tracker.TotalSalePrice}");
                                }
                                ClosedOrder closedOrder = new ClosedOrder(savedOrder);
                                ClosedOrders.Add(closedOrder);
                                CustomerBalance = 0;
                                TenderedAmount = 0;
                                CurrentOrderCheckoutDialog.close();
                                TotalAmount = 0;
                                NoOfItems = 0;
                                SelectedClosedOrder = closedOrder;
                                SelectedCategoryProducts.Clear();
                                MessageBox.Show("Order successfully processed and saved.", "Order Checkout Successful!");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Your session might have expired. Make sure you have authorization to process customer orders.", "Action Requires Authorization!", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Order processing failed. Verify tendered amount and try again.", "INVALID ORDER CHECKOUT!");
                }
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
            if (CurrentOrder != null && CurrentOrder.OrderStatus == 1)
            {
                ShowOrderCheckoutDialog();
                return;
            }
            else
            {
                if (SelectedCategoryProducts.Count > 0)
                {
                    ShowOrderCheckoutDialog(); return;
                }
                else
                {
                    MessageBox.Show("You cannot checkout an empty order.", "Order Checkout Failed!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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

        private void ShowTodaysSales()
        {
            SelectedActionHeader = FriendlyDateHelper.TodaysFriendlySalesDate();
            SelectedAction = 0;
            _chartType = 0;
            IsTopSellingCategoriesDataGridVisible = false;
            if (TodaysOrders != null && TodaysOrders.Count > 0)
            {
                IsSalesDataGridVisible = true;
                IsOrdersDataGridVisible = false;
                IsPendingOrdersDataGridVisible = false;
                IsTopSellingItemsDataGridVisible = false;
                IsInventoryTrackerDataGridVisible = false;
                var dailySales = SalesHelper.GroupSalesByOrderList(TodaysOrders, GroupByScope.Day);
                Sales = new ObservableCollection<Sale>(dailySales);
                SelectedSalesAmount = Sales.Sum(x => x.TotalSalesAmount);
                var topSales = Sales.OrderByDescending(x => x.TotalSalesAmount)
                    .Take(3)
                    .ToList();
                TopSellingItems = ListFormatterHelper.ToFriendlyString(topSales, item => $"{item.ItemName} - {CurrencyFormatter.FormatToUgxCurrency(item.TotalSalesAmount)}");
            }
            else
            {
                Sales = new ObservableCollection<Sale>();
                SelectedSalesAmount = 0;
                TopSellingItems = string.Empty;
                IsSalesDataGridVisible= false;
                IsPendingOrdersDataGridVisible = false;
                IsOrdersDataGridVisible = false;
            }

        }

        private void ShowThisWeeksSales()
        {
            SelectedActionHeader = FriendlyDateHelper.ThisWeeksFriendlySalesDate();
            IsSalesDataGridVisible = true;
            IsOrdersDataGridVisible = false;
            IsPendingOrdersDataGridVisible = false;
            IsTopSellingItemsDataGridVisible = false;
            IsTopSellingCategoriesDataGridVisible = false;
            IsInventoryTrackerDataGridVisible = false;
            SelectedAction = 1;
            _chartType = 1;
            if (WeeklyOrders != null && WeeklyOrders.Count > 0)
            {
                var weeklySales = SalesHelper.GroupSalesByOrderList(WeeklyOrders, GroupByScope.Week);
                Debug.WriteLine("Weekly sales size: "
                    + weeklySales.Count());
                Debug.WriteLine("Weekly Orders size: " + WeeklyOrders.Count());
                Sales = new ObservableCollection<Sale>(weeklySales);
                SelectedSalesAmount = Sales.Sum(x => x.TotalSalesAmount);
                var topSales = Sales.OrderByDescending(x => x.TotalSalesAmount)
                    .Take(3)
                    .ToList();
                TopSellingItems = ListFormatterHelper.ToFriendlyString(topSales, item => $"{item.ItemName} - {CurrencyFormatter.FormatToUgxCurrency(item.TotalSalesAmount)}");


            }
        }

        private void ShowThisMonthsSales()
        {
            SelectedActionHeader = FriendlyDateHelper.ThisMonthsFriendlySalesDate();
            IsSalesDataGridVisible = true;
            IsOrdersDataGridVisible = false;
            IsPendingOrdersDataGridVisible = false;
            IsTopSellingItemsDataGridVisible = false;
            IsTopSellingCategoriesDataGridVisible = false;
            IsInventoryTrackerDataGridVisible = false;
            SelectedAction = 2;
            if (MonthlyOrders != null && MonthlyOrders.Count > 0)
            {
                var montlySales = SalesHelper.GroupSalesByOrderList(MonthlyOrders, GroupByScope.Month);
                Sales = new ObservableCollection<Sale>(montlySales);
                SelectedSalesAmount = Sales.Sum(x => x.TotalSalesAmount);
                var topSales = Sales.OrderByDescending(x => x.TotalSalesAmount)
                    .Take(3)
                    .ToList();
                TopSellingItems = ListFormatterHelper.ToFriendlyString(topSales, item => $"{item.ItemName} - {CurrencyFormatter.FormatToUgxCurrency(item.TotalSalesAmount)}");
            }

        }

        private void ShowChartDialog()
        {
            if (Sales != null && Sales.Count > 0)
            {
                List<Sale> selectedSales = new List<Sale>(Sales);
                ChartViewModel chartViewModel = new ChartViewModel(dataObjects);
                ChartControlDialog chartDialog =new ChartControlDialog(chartViewModel);
                Dialog.Show(chartDialog);
            }
        }

        private void ShowPaymentsChartDialog()
        {
            MobilePaymentsChartViewModel chartViewModel=new MobilePaymentsChartViewModel(dataObjects);
            MobilePaymentsChartDialog dialog =new MobilePaymentsChartDialog(chartViewModel);
            Dialog.Show(dialog);
        }

        private void ShowClosedOrders()
        {
            IsTopSellingItemsDataGridVisible = false;
            IsPendingOrdersDataGridVisible = false;
            IsTopSellingCategoriesDataGridVisible = false;
            IsInventoryTrackerDataGridVisible = false;
            if (CustomerOrders.Count > 0)
            {
                SelectedActionHeader = "Closed Orders";
                IsOrdersDataGridVisible = true;
                IsSalesDataGridVisible = false;
                TotalCashSalesAmount = CustomerOrders.Where(order => order.PreferredPaymentMode == 0)
                    .Sum(x => x.TotalAmountCents);
                TotalMobileMoneySalesAmount = CustomerOrders.Where(order => order.PreferredPaymentMode == 1)
                    .Sum(x => x.TotalAmountCents);
            }
            else
            {
                MessageBox.Show("No closed orders for today.", "No Closed Orders", MessageBoxButton.OK, MessageBoxImage.Information);
                if ((WeeklyOrders != null && WeeklyOrders.Count > 0) && SelectedAction == 0 || SelectedAction == 1)
                {
                    SelectedActionHeader = FriendlyDateHelper.ThisWeeksFriendlySalesDate();
                    IsSalesDataGridVisible = true;
                    IsOrdersDataGridVisible = false;
                    IsPendingOrdersDataGridVisible = false;
                    SelectedAction = 1;

                    var weeklySales = SalesHelper.GroupSalesByOrderList(WeeklyOrders, GroupByScope.Week);
                    Debug.WriteLine("Weekly sales size: "
                        + weeklySales.Count());
                    Debug.WriteLine("Weekly Orders size: " + WeeklyOrders.Count());
                    Sales = new ObservableCollection<Sale>(weeklySales);
                    SelectedSalesAmount = Sales.Sum(x => x.TotalSalesAmount);
                    var topSales = Sales.OrderByDescending(x => x.TotalSalesAmount)
                        .Take(3)
                        .ToList();
                    TopSellingItems = ListFormatterHelper.ToFriendlyString(topSales, item => $"{item.ItemName} - {CurrencyFormatter.FormatToUgxCurrency(item.TotalSalesAmount)}");
                }
                else if (MonthlyOrders != null && MonthlyOrders.Count > 0 && SelectedAction == 2)
                {
                    SelectedActionHeader = FriendlyDateHelper.ThisMonthsFriendlySalesDate();
                    IsSalesDataGridVisible = true;
                    IsOrdersDataGridVisible = false;
                    IsPendingOrdersDataGridVisible = false;
                    SelectedAction = 2;
                    var montlySales = SalesHelper.GroupSalesByOrderList(MonthlyOrders, GroupByScope.Month);
                    Sales = new ObservableCollection<Sale>(montlySales);
                    SelectedSalesAmount = Sales.Sum(x => x.TotalSalesAmount);
                    var topSales = Sales.OrderByDescending(x => x.TotalSalesAmount)
                        .Take(3)
                        .ToList();
                    TopSellingItems = ListFormatterHelper.ToFriendlyString(topSales, item => $"{item.ItemName} - {CurrencyFormatter.FormatToUgxCurrency(item.TotalSalesAmount)}");
                }
            }
        }

        private void ShowPausedOrders()
        {
            if (PendingOrders.Count > 0)
            {
                SelectedActionHeader = "Held Orders";
                IsPendingOrdersDataGridVisible = true;
                IsOrdersDataGridVisible = false;
                IsSalesDataGridVisible = false;
                IsTopSellingItemsDataGridVisible = false;
                IsTopSellingCategoriesDataGridVisible = false;                
            }
            else
            {
                MessageBox.Show("No held orders found.", "No Held Orders", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ShowPaymentMethods()
        {
            IsSalesDataGridVisible = false;
            MessageBox.Show("Show order payment methods");

        }

        private void ShowTopSellingProducts()
        {
            IsOrdersDataGridVisible = false;
            IsSalesDataGridVisible=false;
            IsPendingOrdersDataGridVisible = false;
            IsTopSellingCategoriesDataGridVisible = false;
            IsInventoryTrackerDataGridVisible = false;
            IsTopSellingItemsDataGridVisible = true;
            SelectedActionHeader = "Top Selling Products";
            SalesTypeHeader = $"Top Selling Products - ({SelectedProductDateRange.DisplayName})" ?? "Top Selling Products";
            if (TopSellingCategoryProducts != null && TopSellingCategoryProducts.Count > 0)
            {
                UpdateTopSellingProductNumbers();
                TotalSalesAmountValue = TopSellingCategoryProducts.Sum(x => x.TotalSalesAmount);
            }
            else
            {
                TotalSalesAmountValue = 0;
            }
        }

        private void ShowTopSellingCategories()
        {
            IsOrdersDataGridVisible = false;
            IsSalesDataGridVisible = false;
            IsPendingOrdersDataGridVisible = false;
            IsTopSellingItemsDataGridVisible = false;
            IsInventoryTrackerDataGridVisible = false;
            IsTopSellingCategoriesDataGridVisible = true;
            SelectedActionHeader = "Top Selling Categories";
            SalesTypeHeader = $"Top Selling Categories - ({SelectedDateRange.DisplayName})" ?? "Top Selling Categories";
            if (TopSellingCategories != null && TopSellingCategories.Count > 0)
            {
                TotalSalesAmountValue = TopSellingCategories.Sum(x => x.TotalSalesAmount);
            }
            else
            {
                TotalSalesAmountValue = 0;
            }
        }

        private async Task ShowTopSellingCategoriesByDateRange()
        {
            TopSellingCategories = await dataObjects.GetTopSellingCategoriesByScopeAsync(SelectedDateRange.GroupByScope, SelectedDateRange.StartDate, SelectedDateRange.EndDate);
            if(TopSellingCategories.Count > 0)
            {
                TotalSalesAmountValue = TopSellingCategories.Sum(x => x.TotalSalesAmount);
                IsOrdersDataGridVisible = false;
                IsPendingOrdersDataGridVisible = false;
                IsSalesDataGridVisible = false;
                IsTopSellingItemsDataGridVisible = false;
                IsInventoryTrackerDataGridVisible = false;
                IsTopSellingCategoriesDataGridVisible = true;
                SelectedActionHeader = $"Top Selling Categories for {DateRangeDisplay}";
            }
            else
            {
                TotalSalesAmountValue = 0;
                
            }

        }

        private async Task ShowTopSellingProductsByDateRange()
        {
            TopSellingCategoryProducts = await dataObjects.GetTopSellingCategoryProductsByScope(SelectedProductDateRange.GroupByScope, SelectedProductDateRange.StartDate, SelectedProductDateRange.EndDate);
            if (TopSellingCategoryProducts.Count > 0)
            {
                IsOrdersDataGridVisible = false;
                IsPendingOrdersDataGridVisible = false;
                IsSalesDataGridVisible = false;
                IsTopSellingCategoriesDataGridVisible = false;
                IsInventoryTrackerDataGridVisible = false;
                IsTopSellingItemsDataGridVisible = true;
                SelectedActionHeader = $"Top Selling Products for {ProductDateRangeDisplay}";
                UpdateTopSellingProductNumbers();

                TotalSalesAmountValue = TopSellingCategoryProducts.Sum(x => x.TotalSalesAmount);
            }
            else
            {
                TotalSalesAmountValue = 0;
            }
        }

        public string DateRangeDisplay=> $"{SelectedDateRange.StartDate:dd MMM yyyy} - {SelectedDateRange.EndDate:dd MMM yyyy}";
        public string ProductDateRangeDisplay => $"{SelectedProductDateRange.StartDate:dd MMM yyyy} - {SelectedProductDateRange.EndDate:dd MMM yyyy}";

        private void ShowInventoryTracker()
        {
            IsOrdersDataGridVisible = false;
            IsSalesDataGridVisible = false;
            IsPendingOrdersDataGridVisible = false;
            IsTopSellingItemsDataGridVisible = false;
            IsTopSellingCategoriesDataGridVisible = false;
            IsInventoryTrackerDataGridVisible = true;
            SelectedActionHeader = "Inventory";
        }

        private void ShowStockItemInventoryLog()
        {
            InventoryTrackerViewModel inventoryTrackerViewModel =
                new InventoryTrackerViewModel(dataObjects, SelectedStockItem);
            InventoryLogDialog inventoryLogDialog = new InventoryLogDialog(inventoryTrackerViewModel);           
            HandyControl.Controls.Dialog.Show(inventoryLogDialog);
        }

        private void ShowRestockItemDialog()
        {
            InventoryTrackerViewModel inventoryTrackerViewModel =
                new InventoryTrackerViewModel(dataObjects, SelectedStockItem);
            inventoryTrackerViewModel.RestockCompleteHandler += OnRestockCompleted;
            restockItemDialog =
                new RestockItemDialog(inventoryTrackerViewModel);
            Dialog.Show(restockItemDialog);
        }

        private void ShowEditProductDialog()
        {
            ProductViewModel productViewModel=new ProductViewModel(dataObjects, _services, SelectedStockItem.Id);
            productViewModel.ProductUpdateCompleteHandler += OnProductUpdateComplete;
            editProductDialog = new EditProductDialog(productViewModel);            
            Dialog.Show(editProductDialog);
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private void OnProductUpdateComplete(object sender, bool value)
        {
            if (value)
            {
                // Refresh stock items after product update
             
                UpdateStockItemNumbers();
                MessageBox.Show("Product successfully updated.", "Product Update Successful!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Product update failed. Please try again.", "Product Update Failed!", MessageBoxButton.OK, MessageBoxImage.Error);
                editProductDialog.close();
            }
        }

        private void OnRestockCompleted(object sender, bool value)
        {
            if (value)
            {
                // Remove iten restocked item 
                StockItems.Remove(SelectedStockItem);
                // Refresh stock items after restock
                UpdateStockItemNumbers();
                MessageBox.Show("Item successfully restocked.", "Restock Successful!", MessageBoxButton.OK, MessageBoxImage.Information);
                restockItemDialog.close();
            }
            else
            {
                MessageBox.Show("Restock failed. Please try again.", "Restock Failed!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
