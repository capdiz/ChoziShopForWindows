using ChoziShop.Data;
using ChoziShopForWindows.Views;
using ChoziShopForWindows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ChoziShop.Data.Models;
using HandyControl.Controls;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ChoziShop.Data.Repository;
using HandyControl.Interactivity;
using ChoziShopForWindows.MerchantsApi;
using ChoziShopForWindows.models;
using ChoziShopForWindows.Services;
using Microsoft.Extensions.DependencyInjection;
using ChoziShopSharedConnectivity.Shared;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Threading.Tasks.Dataflow;
using MessageBox = System.Windows.MessageBox;
using DefaultUserAccount = ChoziShopForWindows.models.DefaultUserAccount;
using System.Security;
using System.Net;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace ChoziShopForWindows.ViewModels
{
    public class MainWindowViewModel : ObservableObject, INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private readonly ILogger _logger;
        private readonly IDataObjects _dataObjects;
        private readonly DatabaseWatcher _databaseWatcher;
        private readonly InternetConnectivityMonitorService _internetConnectivityMonitorService;
        private readonly BufferBlock<ConnectivityStatus> _statusQueue = new BufferBlock<ConnectivityStatus>();
       
        private String _currentTime;
        private DispatcherTimer _dispatchTimer;
        private DatabaseManager choziShopDatabaseManager;
        private IServiceScope _currentScope;
        private IAuthTokenProvider _authTokenProvider;
       
        private SessionManager _sessionManager;

        private BitmapSource _qrCodeImage;

        private bool isMerchantAccountActivated = false;
        // Know if current session is a merchant session
        private bool isMerchantSessionActive = false;
        // know if app is having an active user session
        private bool isUserSessionActive = false;

        private bool _isStoreHavingKeeperAccounts = false;
        private bool _isInternetConnected;
        private bool _isXmppConnected;
        private bool _isInternetNotConnected;
        private bool _isSyncingUnsyncedObjects = false;
        private bool _isLoadingCircleVisible = false;
        private bool _isControlEnabled = true;
        private bool _isKeeperVerificationStatusVisible = false;
        private bool _isKeeperVerificationSuccessful = false;
        private bool _isStockStatusHeaderVisible = false;

        private ConnectivityStatus _connectivityStatus;

        private string _selectedViewName = "HomeView";
        private List<string> _countryOfOperations;

        private CreateMerchantAccountDialog CreateMerchantAccountDialog;
        private UserLoginDialog UserLoginDialog;
        private AddKeeperPasswordDialog _passwordDialog;

        IServiceProvider _services;

        private Visibility _isHomeViewVisible = Visibility.Visible;
        private Visibility _isEasyPosViewVisible = Visibility.Collapsed;
        private Visibility _isShopsViewVisible = Visibility.Collapsed;
        private Visibility _isCreateShopViewVisible = Visibility.Collapsed;
        private Visibility _isShopKeepersViewVisible = Visibility.Collapsed;
        private Visibility _isOrdersViewVisible = Visibility.Collapsed;
        private Visibility _isInventoryViewVisible = Visibility.Collapsed;
        private Visibility _isPaymentsViewVisible = Visibility.Collapsed;
        private Visibility _isScheduledOrdersViewVisible = Visibility.Collapsed;
        private Visibility _isDiscountsViewVisible = Visibility.Collapsed;
        private Visibility _isPromotionsViewVisible = Visibility.Collapsed;
        private Visibility _isSettingsViewVisible = Visibility.Collapsed;
        public Visibility _isProgressBarVisible = Visibility.Collapsed;

        private Merchant savedMerchant;
        private MerchantAccount currentMerchant;
        
        private Keeper _verifiedKeeper;
        private KeeperAccount _keeperAccount;

        private SecureString _password;

        private readonly Dictionary<string, List<string>> _errors = new();

        private string _verifiedKeeperHeader;
        private string _keeperEmail;

        private Store _currentStore;

        private int _storeSetupStatus = 0;
        private string storeSetupStatusText;
        private string currentSessionStatus = "Inactive Session";
        private string currentUserRole = "Unknown User";
        private string _keeperVerificationStatus;
        private string _currentUserName = string.Empty;

        private int _verificationCode;
        private int _loggedInUserAccountType;
        private object _currentUserControl;
       
        private BarcodeGenerator barcodeGenerator;

        private List<Store> _merchantStores;

        public EventHandler<bool> InternetStatusHandler;
        private HomeView _homeView;

        private string _stockStatusHeader;
        private List<CategoryProduct> _inventoryCategoryProducts;

        public MainWindowViewModel(IDataObjects dataObjects, IServiceProvider services)
        {
            _logger = Log.ForContext<MainWindowViewModel>();

            InitializeTimer();

            _dataObjects = dataObjects;
            _services = services;

            CountryOfOperations = new List<string>
            {
                "+256",
                "+254",
                "+255",
                "+250",
                "+250",
                "Burundi",
                "South Sudan"
            };

            choziShopDatabaseManager = new DatabaseManager(DbFileConfig.FullDbOPath, "Merchants");
            CurrentMerchantAccount = choziShopDatabaseManager.GetMerchant();

            LoadDefaultAccountStore();
            LoadInventoryCategoryProducts();
            LoadStockInventoryStatus();


            ShowCurrentUserControlCommand = new Commands.RelayCommand(param => ShowSelectedView(param));
            GenerateCodeCommand = new Commands.RelayCommand(_ => GenerateCode());
            VerifyKeeperCodeCommand = new Commands.RelayCommand(_ => VerifyKeeperInvitationCode(VerificationCode.ToString()));

            LoginKeeperCommand = new Commands.RelayCommand(_ => LoginKeeper());

            IsLoadingCircleVisible = false;
            IsControlEnabled = true;
            IsKeeperVerificationStatusVisible = false;


            IsMerchantAccountActivated = false;
            barcodeGenerator = new BarcodeGenerator(this);
            barcodeGenerator.MerchantResponseHandler += OnMerchantReceived;

            isMerchantSessionActive = choziShopDatabaseManager.IsMerchantSessionActive();
            IsUserSessionActive = false;
            _currentScope = _services.CreateScope();
            _authTokenProvider = services.GetRequiredService<IAuthTokenProvider>();
            _authTokenProvider.SetCurrentMerchantAccount(CurrentMerchantAccount);           
            _internetConnectivityMonitorService = services.GetRequiredService<InternetConnectivityMonitorService>();
            // Subscribe to the Internet StatusChanged event
            _internetConnectivityMonitorService.StatusChanged += OnInternetStatusChanged;

            _databaseWatcher = services.GetRequiredService<DatabaseWatcher>();

            // Process queue at max 10 FPS
            Task.Run(async () =>
            {
                while (true)
                {
                    var status = await _statusQueue.ReceiveAsync();
                    Debug.WriteLine("The status in WPF is: " + status.IsInternetConnected);
                    UpdateConnectivityStatus(status);
                    await Task.Delay(100); // 10 FPS
                }
            });


        }

        async void LoadInventoryCategoryProducts()
        {
            _inventoryCategoryProducts = await _dataObjects.GetCategoryProductsAsync();
        }

        private void OnInternetStatusChanged(ConnectivityStatus status)
        {
            IsInternetConnected = status.IsInternetConnected;
            InternetStatusHandler?.Invoke(this, status.IsInternetConnected);
            _statusQueue.Post(status);           
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

        public bool IsMerchantAccountActivated
        {
            get { return isMerchantAccountActivated; }
            set
            {
                isMerchantAccountActivated = value;
                OnPropertyChanged();
            }
        }

        public bool IsMerchantSessionActive
        {
            get { return isMerchantSessionActive; }
            set
            {
                isMerchantSessionActive = value;
                OnPropertyChanged();
            }
        }

        public bool IsStoreHavingKeeperAccounts
        {
            get { return _isStoreHavingKeeperAccounts; }
            set
            {
                _isStoreHavingKeeperAccounts = value;
                OnPropertyChanged();
            }
        }

        public bool IsUserSessionActive
        {
            get { return isUserSessionActive; }
            set
            {
                isUserSessionActive = value;
                OnPropertyChanged();
            }
        }

        public bool IsInternetConnected
        {
            get { return _isInternetConnected; }
            set
            {
                IsInternetNotConnected = !value;
                SetField(ref _isInternetConnected, value);
            }
        }

        public bool IsInternetNotConnected
        {
            get { return _isInternetNotConnected; }
            set
            {
                _isInternetNotConnected = value;
                OnPropertyChanged();
            }
        }

        public bool IsXmppConnected
        {
            get { return _isXmppConnected; }
            set
            {
                SetField(ref _isXmppConnected, value);
            }
        }

        public bool IsSyncingUnsyncedObjects
        {
            get { return _isSyncingUnsyncedObjects; }
            set
            {
                SetField(ref _isSyncingUnsyncedObjects, value);
            }
        }

        public bool IsLoadingCircleVisible
        {
            get { return _isLoadingCircleVisible; }
            set
            {
                _isLoadingCircleVisible = value;
                OnPropertyChanged();
            }
        }

        public bool IsControlEnabled
        {
            get { return _isControlEnabled; }
            set
            {
                _isControlEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool IsKeeperVerificationStatusVisible
        {
            get { return _isKeeperVerificationStatusVisible; }
            set
            {
                _isKeeperVerificationStatusVisible = value;
                OnPropertyChanged();
            }
        }

        public bool IsKeeperVerificationSuccessful
        {
            get { return _isKeeperVerificationSuccessful; }
            set
            {
                _isKeeperVerificationSuccessful = value;
                OnPropertyChanged();
            }
        }

        public bool IsStockStatusHeaderVisible
        {
            get { return _isStockStatusHeaderVisible; }
            set
            {
                _isStockStatusHeaderVisible = value;
                OnPropertyChanged();
            }
        }

        public ConnectivityStatus ConnectivityStatus
        {
            get { return _connectivityStatus; }
            set
            {
                SetField(ref _connectivityStatus, value);
            }
        }

        public string SelectedViewName
        {
            get { return _selectedViewName; }
            set
            {
                _selectedViewName = value;
                OnPropertyChanged();
            }
        }


        public string CurrentTime
        {
            get { return _currentTime; }
            set
            {
                _currentTime = value;
                OnPropertyChanged();
            }
        }

        public List<string> CountryOfOperations
        {
            get { return _countryOfOperations; }
            set
            {
                _countryOfOperations = value;
                OnPropertyChanged();
            }
        }

        public string CurrentSessionStatus
        {
            get { return currentSessionStatus; }
            set
            {
                currentSessionStatus = value;
                OnPropertyChanged();
            }
        }

        public string CurrentUserRole
        {
            get { return currentUserRole; }
            set
            {
                currentUserRole = value;
                OnPropertyChanged();
            }
        }

        public string KeeperVerificationStatus
        {
            get { return _keeperVerificationStatus; }
            set
            {
                _keeperVerificationStatus = value;
                OnPropertyChanged();
            }
        }

        public string CurrentUserName
        {
            get { return _currentUserName; }
            set
            {
                _currentUserName = value;
                OnPropertyChanged();
            }
        }

        public string StockStatusHeader
        {
            get { return _stockStatusHeader; }
            set
            {
                _stockStatusHeader = value;
                OnPropertyChanged();
            }
        }


        public Visibility IsHomeViewVisible
        {
            get { return _isHomeViewVisible; }
            set
            {
                _isHomeViewVisible = value;
                OnPropertyChanged();
            }
        }

        public Visibility IsEasyPosviewVisible
        {
            get
            {
                return _isEasyPosViewVisible;
            }
            set
            {
                _isEasyPosViewVisible = value;
                OnPropertyChanged();
            }
        }

        public Visibility IsShopKeepersViewVisible
        {
            get
            {
                return _isShopKeepersViewVisible;
            }
            set
            {
                _isShopKeepersViewVisible = value;
                OnPropertyChanged();
            }
        }

        public Visibility IsShopsViewVisible
        {
            get
            {
                return _isShopsViewVisible;
            }
            set
            {
                _isShopsViewVisible = value;
                OnPropertyChanged();
            }
        }

        public Visibility IsCreateShopViewVisible
        {
            get
            {
                return _isCreateShopViewVisible;
            }
            set
            {
                _isCreateShopViewVisible = value;
                OnPropertyChanged();
            }
        }

        public Visibility IsOrdersViewVisible
        {
            get
            {
                return _isOrdersViewVisible;
            }
            set
            {
                _isOrdersViewVisible = value;
                OnPropertyChanged();
            }
        }

        public Visibility IsInventoryViewVisible
        {
            get
            {
                return _isInventoryViewVisible;
            }
            set
            {
                _isInventoryViewVisible = value;
                OnPropertyChanged();
            }
        }

        public Visibility IsPaymentsViewVisible
        {
            get
            {
                return _isPaymentsViewVisible;
            }
            set
            {
                _isPaymentsViewVisible = value;
                OnPropertyChanged();
            }
        }

        public Visibility IsScheduledOrdersViewVisible
        {
            get
            {
                return _isScheduledOrdersViewVisible;
            }
            set
            {
                _isScheduledOrdersViewVisible = value;
                OnPropertyChanged();
            }
        }

        public Visibility IsDiscountsViewVisible
        {
            get
            {
                return _isDiscountsViewVisible;
            }
            set
            {
                _isDiscountsViewVisible = value;
                OnPropertyChanged();
            }
        }

        public Visibility IsPromotionsViewVisible
        {
            get
            {
                return _isPromotionsViewVisible;
            }
            set
            {
                _isPromotionsViewVisible = value;
                OnPropertyChanged();
            }
        }

        public Visibility IsSettingsViewVisible
        {
            get
            {
                return _isSettingsViewVisible;
            }
            set
            {
                _isSettingsViewVisible = value;
                OnPropertyChanged();
            }
        }

        public Visibility IsProgressBarVisible
        {
            get
            {
                return _isProgressBarVisible;
            }
            set
            {
                _isProgressBarVisible = value;
                OnPropertyChanged();
            }
        }

        public object CurrentUserControl
        {
            get { return _currentUserControl; }
            set
            {

                _currentUserControl = value;
                OnPropertyChanged();
                Debug.WriteLine("CurrentUserControl updated to: " + value?.GetType().Name);
            }
        }

        public MerchantAccount CurrentMerchantAccount
        {
            get { return currentMerchant; }
            set
            {
                currentMerchant = value;
                OnPropertyChanged();
            }
        }

        public KeeperAccount CurrentKeeperAccount
        {
            get { return _keeperAccount; }
            set
            {
                _keeperAccount = value;
                OnPropertyChanged();
            }
        }

        public Keeper VerifiedKeeper
        {
            get { return _verifiedKeeper; }
            set
            {
                _verifiedKeeper = value;
                OnPropertyChanged();
            }
        }


        [Required(ErrorMessage = "Your email is required")]
        [StrictEmail(ErrorMessage = "Invalid email address")]
        public string KeeperEmail
        {
            get { return _keeperEmail; }
            set
            {
                _keeperEmail = value;
                ValidateProperty(value);
                OnPropertyChanged();
            }
        }

        public SecureString Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged();
                ValidatePasswordConfirmation();
            }
        }
      
        public Store CurrentStore
        {
            get { return _currentStore; }
            set
            {
                _currentStore = value;
                OnPropertyChanged();
            }
        }

        public BitmapSource QrCodeImage
        {
            get { return _qrCodeImage; }
            set
            {
                _qrCodeImage = value;
                OnPropertyChanged();
            }
        }

        public Merchant SavedMerchant
        {
            get { return savedMerchant; }
            set
            {
                savedMerchant = value;
                OnPropertyChanged();
            }
        }

        public int StoreSetupStatus
        {
            get { return _storeSetupStatus; }
            set
            {
                _storeSetupStatus = value;
                OnPropertyChanged();
            }
        }

        public string StoreSetupStatusText
        {
            get { return storeSetupStatusText; }
            set
            {
                storeSetupStatusText = value;
                OnPropertyChanged();
            }
        }

        public List<Store> MerchantStores
        {
            get { return _merchantStores; }
            set
            {
                _merchantStores = value;
                OnPropertyChanged();
            }
        }

        public int VerificationCode
        {
            get { return _verificationCode; }
            set { _verificationCode=value; OnPropertyChanged(); }   
        }

        public AddKeeperPasswordDialog KeeperPasswordDialog
        {
            get
            {
                return _passwordDialog;
            }
            set
            {
                _passwordDialog = value;
                OnPropertyChanged();
            }
        }


        public ICommand ShowCurrentUserControlCommand { get; }

        public ICommand GenerateCodeCommand { get; }

        public ICommand SaveMerchantDetailsCommand { get; }

        public ICommand VerifyKeeperCodeCommand { get; }
        
        public ICommand LoginKeeperCommand { get; }

        private void GenerateCode()
        {
            QrCodeImage = barcodeGenerator.GenerateBarcodeImage();
        }

        private void GenerateMerchantLoginQrCode()
        {
            if (CurrentMerchantAccount != null && ApplicationState.Instance.DefaultAccount==null)
            {
                var baseApi = new BaseApi(CurrentMerchantAccount.AuthToken);
                barcodeGenerator = new BarcodeGenerator(baseApi);
                barcodeGenerator.SetMainWindowViewModel(this);
                if (isMerchantSessionActive)
                {
                    barcodeGenerator.SetIsMerchantSessionActive(isMerchantSessionActive);
                    barcodeGenerator.SetSessionAuthToken(choziShopDatabaseManager.GetSessionAuthToken());
                    barcodeGenerator.SetActiveSessionId(choziShopDatabaseManager.GetSessionId());
                }
                else
                {
                    if (!IsInternetConnected)
                    {
                        MessageBox.Show("Please connect to the internet to generate a login QR code.", "No Internet Connection", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                barcodeGenerator.WindowsSessionResponseHandler += OnSessionActive;
                QrCodeImage = barcodeGenerator.GenerateLoginBarcode();
            }
        }

        

        private async void LoadDefaultAccountStore()
        {
            CurrentStore = await _dataObjects.GetDefaultUserAccountStore();
            ApplicationState.Instance.CurrentStore = CurrentStore;
        }

        private void LoadStockInventoryStatus()
        {
            List<StockItem> _outOfStockItems =
                _dataObjects.GroupStockItemsByStatusAsync(_inventoryCategoryProducts, Enums.ProductInventoryStatus.Empty);
            List<StockItem> _criticalOnStockItems =
                _dataObjects.GroupStockItemsByStatusAsync(_inventoryCategoryProducts, Enums.ProductInventoryStatus.Critical);
            List<StockItem> _lowOnStockItems =
                _dataObjects.GroupStockItemsByStatusAsync(_inventoryCategoryProducts, Enums.ProductInventoryStatus.Low);
            IsStockStatusHeaderVisible = false ? _outOfStockItems.Count == 0 && _criticalOnStockItems.Count == 0 && _lowOnStockItems.Count == 0 : true;
            StockStatusHeader = StockStatusFormatter.FormatStockStatus(_outOfStockItems.Count, _criticalOnStockItems.Count, _lowOnStockItems.Count);
            IsStockStatusHeaderVisible = true ? _outOfStockItems.Count > 0 || _criticalOnStockItems.Count > 0 || _lowOnStockItems.Count > 0 : false;
        }

        private async void OnSessionActive(object sender, WindowsSessionResponse sessionResponse)
        {
            Debug.WriteLine("Successfully received session response");
            if (sessionResponse != null)
            {
                if (sessionResponse.Status.Equals("pending"))
                {
                    Debug.WriteLine("Session is pending activation. Please wait...");
                    ActivateMerchantSession(sessionResponse);
                }
                else if (sessionResponse.Status.Equals("active"))
                {
                    if (choziShopDatabaseManager != null)
                    {
                        // Session was just restarted
                        if (choziShopDatabaseManager.GetSessionAuthToken() == sessionResponse.AuthToken)
                        {
                            await _dataObjects.MerchantSessions.UpdateSingleColumnAsync(
                                predicate: session => session.SessionId == sessionResponse.Id,
                                updateAction: entity => entity.ExpiresAt = sessionResponse.ExpiresAt);
                            await Application.Current.Dispatcher.InvokeAsync(async () =>
                            {
                                // start monitoring session from here                                
                                _sessionManager = new SessionManager(sessionExpiresAt: sessionResponse.ExpiresAt.ToString());
                                _sessionManager.SessionExpired += OnSessionExpired;
                                _sessionManager.StartSessionMonitoring();

                                barcodeGenerator.SetIsMerchantSessionActive(true);
                                IsMerchantSessionActive = true;
                                IsUserSessionActive = true;
                                CurrentSessionStatus = "Active Session";
                                CurrentUserRole = "Merchant";                                
                                ApplicationState.Instance.DefaultAccount = new DefaultUserAccount(CurrentMerchantAccount);                               
                                LoggedInUserAccountType = 1; // Merchant account type   
                                // Stop the authorization timer if it was running
                                barcodeGenerator.StopAuthorizationTimer();
                                CurrentUserName = ApplicationState.Instance.DefaultAccount.FullName;
                                // Stop the authorization timer if it was running
                                barcodeGenerator.StopAuthorizationTimer();

                                // Set current user store
                                if (CurrentStore != null)
                                {
                                    ApplicationState.Instance.DefaultAccount.SetCurrentUserStore(CurrentStore);
                                }
                                else { 
                                    CurrentStore = await _dataObjects.GetDefaultUserAccountStore();
                                    ApplicationState.Instance.DefaultAccount.SetCurrentUserStore(CurrentStore); 
                                }


                                OrdersViewModel _ordersViewModel = _services.GetRequiredService<OrdersViewModel>();
                                _ordersViewModel.SetLoggedInUserAccountType(LoggedInUserAccountType);
                                CurrentUserControl = _services.GetRequiredService<HomeView>();                                
                                Debug.WriteLine("Merchant session successfully reactivated");
                                UserLoginDialog.close();
                            });
                        }
                        else
                        {
                            MerchantSession merchantSession = new MerchantSession
                            {
                                SessionId = sessionResponse.Id,
                                AuthToken = sessionResponse.AuthToken,
                                DeviceToken = sessionResponse.DeviceToken,
                                Status = sessionResponse.Status,
                                ExpiresAt = sessionResponse.ExpiresAt
                            };

                            MerchantSession savedSession = await _dataObjects.MerchantSessions.SaveAndReturnEntityAsync(merchantSession);
                            await Application.Current.Dispatcher.InvokeAsync(async () =>
                            {
                                if (savedSession != null)
                                {
                                    // start monitoring session from here                                    
                                    _sessionManager = new SessionManager(sessionExpiresAt: sessionResponse.ExpiresAt.ToString());
                                    _sessionManager.SessionExpired += OnSessionExpired;
                                    _sessionManager.StartSessionMonitoring();

                                    barcodeGenerator.SetIsMerchantSessionActive(true);
                                    IsMerchantSessionActive = true;
                                    IsUserSessionActive = true;
                                    CurrentSessionStatus = "Active Session";
                                    CurrentUserRole = "Merchant";
                                    ApplicationState.Instance.DefaultAccount = new DefaultUserAccount(CurrentMerchantAccount);
                                    LoggedInUserAccountType = 1; // Merchant account type
                                    // Stop authorization timer if its running
                                    barcodeGenerator.StopAuthorizationTimer();
                                    CurrentUserName = ApplicationState.Instance.DefaultAccount.FullName;

                                    // set current user store
                                    if (CurrentStore != null)
                                    {
                                        ApplicationState.Instance.DefaultAccount.SetCurrentUserStore(CurrentStore);
                                    }
                                    else
                                    {
                                        CurrentStore = await _dataObjects.GetDefaultUserAccountStore();
                                        ApplicationState.Instance.DefaultAccount.SetCurrentUserStore(CurrentStore);
                                    }

                                    OrdersViewModel _ordersViewModel = _services.GetRequiredService<OrdersViewModel>();
                                    _ordersViewModel.SetLoggedInUserAccountType(LoggedInUserAccountType);                                    
                                    CurrentUserControl = _services.GetRequiredService<HomeView>();
                                    Debug.WriteLine("Session is activated. Please wait...");
                                    UserLoginDialog.close();
                                }
                            });
                        }
                    }
                }

            }
        }

        private async void OnMerchantReceived(object sender, MerchantResponse merchantResponse)
        {
            Debug.WriteLine("Successfully received merchant response");
            if (merchantResponse != null)
            {
                Debug.WriteLine("Merchant response is: " + merchantResponse.FullName);
                var merchant = new Merchant()
                {
                    OnlineMerchantId = merchantResponse.OnlineMerchantId,
                    FullName = merchantResponse.FullName,
                    Email = merchantResponse.Email,
                    PhoneNumber = merchantResponse.PhoneNumber,
                    CountryOfOperations = merchantResponse.CountryOfOperations,
                    AuthPassword = merchantResponse.AuthPassword,
                    CreatedAt = merchantResponse.CreatedAt,
                    UpdatedAt = merchantResponse.UpdatedAt,
                    AuthToken = merchantResponse.AuthToken,
                    BareJid = merchantResponse.BareJid,
                    FullJid = merchantResponse.FullJid
                };
                Merchant createdMerchant = _dataObjects.SaveAndReturnMerchantAsync(merchant).Result;
                SavedMerchant = createdMerchant;
                if (SavedMerchant.Id > 0)
                {
                    await Application.Current.Dispatcher.InvokeAsync(async () =>
                    {
                        IsMerchantAccountActivated = true;
                        IsProgressBarVisible = Visibility.Visible;
                        StoreSetupStatus = 30;
                        StoreSetupStatusText = "Fetching stores...";
                        if (CreateMerchantAccountDialog != null)
                        {
                            isUserSessionActive = true;
                            CurrentMerchantAccount = new MerchantAccount
                            {
                                OnlineMerchantId = createdMerchant.OnlineMerchantId,
                                FullName = createdMerchant.FullName,
                                Email = createdMerchant.Email,
                                PhoneNumber = createdMerchant.PhoneNumber,
                                AuthToken = createdMerchant.AuthToken,
                                BareJid = createdMerchant.BareJid,
                                FullJid = createdMerchant.FullJid
                            };
                            CurrentUserRole = "Merchant";
                            CurrentUserName = CurrentMerchantAccount.FullName;
                            ApplicationState.Instance.DefaultAccount = new DefaultUserAccount(CurrentMerchantAccount);
                            LoggedInUserAccountType = 1; // Merchant account type
                            // Set current user store
                            if (CurrentStore != null)
                            {
                                ApplicationState.Instance.DefaultAccount.SetCurrentUserStore(CurrentStore);
                            }
                            else
                            {
                                CurrentStore = await _dataObjects.GetDefaultUserAccountStore();
                                ApplicationState.Instance.DefaultAccount.SetCurrentUserStore(CurrentStore);
                            }
                            OrdersViewModel _ordersViewModel = _services.GetRequiredService<OrdersViewModel>(); 
                            _ordersViewModel.SetLoggedInUserAccountType(LoggedInUserAccountType);
                            barcodeGenerator.StopAuthorizationTimer();
                            
                            CurrentUserControl = _services.GetRequiredService<HomeView>();
                            CreateMerchantAccountDialog.close();
                            List<StoreResponse> serializedStores = await FetchMerchantStore();
                            if (serializedStores.Count > 0)
                            {
                                StoreSetupStatus = 50;
                                StoreSetupStatusText = serializedStores.Count + " store(s) found. Preparing inventories..";
                                for (int i = 0; i < serializedStores.Count; i++)
                                {
                                    var serializedStore = serializedStores[i];
                                    var store = new Store
                                    {
                                        OnlineStoreId = serializedStore.Id,
                                        MerchantId = serializedStore.MerchantId,
                                        InventoryId = serializedStore.StoreInventory.Id,
                                        StoreName = serializedStore.StoreName,
                                        CountryOfOperations = serializedStore.CountryOfOperations,
                                        Latitude = serializedStore.Latitude,
                                        Longitude = serializedStore.Longitude,
                                        CreatedAt = serializedStore.CreatedAt,
                                        UpdatedAt = serializedStore.UpdatedAt,
                                        OpeningTime = serializedStore.OpeningTime,
                                        ClosingTime = serializedStore.ClosingTime,
                                        ShopCode = serializedStore.ShopCode,
                                        BaseLocation = serializedStore.BaseLocation,
                                        LocationName = serializedStore.LocationName,
                                        Directions = serializedStore.Directions
                                    };
                                    await _dataObjects.SaveAndReturnStoreAsync(store);
                                    if (_merchantStores == null)
                                    {
                                        _merchantStores = new List<Store>();
                                    }
                                    _merchantStores.Add(store);
                                    MerchantStores = _merchantStores;


                                    if (serializedStore.Id == serializedStores[serializedStores.Count - 1].Id)
                                    {
                                        StoreSetupStatus = 100;
                                        StoreSetupStatusText = "All stores have been set up successfully";
                                        IsProgressBarVisible = Visibility.Collapsed;
                                    }
                                    else
                                    {
                                        StoreSetupStatus = 50 + (i * 10);
                                        StoreSetupStatusText = "Preparing store " + (i + 1) + " of " + serializedStores.Count;
                                    }

                                    StoreSetupStatus = 70 + (i * 10);
                                    StoreSetupStatusText = "Fetching categories for store " + (i + 1) + " of " + serializedStores.Count;
                                    List<CategorySectionResponse> serializedCategories = await FetchStoreCategories(serializedStore.Id);
                                    if (serializedCategories != null)
                                    {
                                        for (int j = 0; j < serializedCategories.Count; j++)
                                        {
                                            var categorySection = new CategorySection
                                            {
                                                OnlineCategorySectionId = serializedCategories[j].Id,
                                                InventoryId = serializedCategories[j].InventoryId,
                                                CategorySectionName = serializedCategories[j].CategoryName,
                                                CreatedAt = serializedCategories[j].CreatedAt,
                                                UpdatedAt = serializedCategories[j].UpdatedAt,
                                            };
                                            CategorySection savedCategorySection = await _dataObjects.AddCategorySectionAsync(categorySection);
                                            if (serializedCategories[j].CategoryProducts != null && serializedCategories[j].CategoryProducts.Count > 0)
                                            {
                                                for (int k = 0; k < serializedCategories[j].CategoryProducts.Count; k++)
                                                {
                                                    var categoryProduct = new CategoryProduct
                                                    {
                                                        CategorySection = savedCategorySection,
                                                        OnlineCategoryProductId = serializedCategories[j].CategoryProducts[k].Id,
                                                        ProductName = serializedCategories[j].CategoryProducts[k].ProductName,
                                                        Remarks = serializedCategories[j].CategoryProducts[k].Remarks,
                                                        Tag = serializedCategories[j].CategoryProducts[k].Tag,
                                                        Measurement = serializedCategories[j].CategoryProducts[k].Measurement,
                                                        Currency = "UGX",
                                                        UnitCost = serializedCategories[j].CategoryProducts[k].UnitCostCents,
                                                        Units = (int)serializedCategories[j].CategoryProducts[k].Units,
                                                        ValueMetric = serializedCategories[j].CategoryProducts[k].ValueMetric,
                                                        CreatedAt = serializedCategories[j].CategoryProducts[k].CreatedAt,
                                                        UpdatedAt = serializedCategories[j].CategoryProducts[k].UpdatedAt,
                                                        BarcodeUrl = serializedCategories[j].CategoryProducts[k].BarcodeUrl,
                                                    };
                                                    Debug.WriteLine("Adding product " + categoryProduct.ProductName + ". With id: " + categoryProduct.OnlineCategoryProductId +
                                                        " to category " + savedCategorySection.CategorySectionName);
                                                    Debug.WriteLine("Category Id: " + savedCategorySection.Id);
                                                    StoreSetupStatus = 80 + (i * 10);
                                                    StoreSetupStatusText = "Adding product " + (k + 1) + " of " + serializedCategories[j].CategoryProducts.Count +
                                                        " to category " + serializedCategories[j].CategoryName;
                                                    await _dataObjects.AddCategoryProductAsync(categoryProduct);
                                                }

                                            }
                                            Debug.WriteLine("Category name: " + serializedCategories[j].CategoryName +
                                                " No. of products in category: " + serializedCategories[j].CategoryProducts.Count);
                                        }
                                        Debug.WriteLine("Serialized categories count: " + serializedCategories.Count);
                                    }
                                }
                                Debug.WriteLine(serializedStores.Count + " stores found");
                            }
                        }
                    });
                }
            }
        }

        private async void VerifyKeeperInvitationCode(string verificationCode)
        {          
            if (verificationCode.Length < 6)
            {
                MessageBox.Show("Please enter a valid verification code", "Invalid Code", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            IsControlEnabled = false;
            IsLoadingCircleVisible = true;
            BaseApi _baseApi = new BaseApi();
            var serializedKeeper = await _baseApi.VerifyKeeperVerificationCode(verificationCode);
            IsKeeperVerificationStatusVisible = true;
            KeeperVerificationStatus = "Verifying code...";
            if (serializedKeeper != null)
            {
                var keeper = await _dataObjects.FindKeeperByOnlineId(serializedKeeper.KeeperId);
                if (keeper != null)
                {
                    IsKeeperVerificationSuccessful = true;
                    IsLoadingCircleVisible = false;
                    KeeperVerificationStatus = "Keeper verified successfully. Create your ChoziShop password";
                    keeper.AuthToken = serializedKeeper.AuthToken;
                    VerifiedKeeper = keeper;
                    UserLoginDialog.close();
                    KeeperViewModel keeperViewModel = new KeeperViewModel(_dataObjects, VerifiedKeeper);
                    KeeperPasswordDialog = new AddKeeperPasswordDialog(keeperViewModel);
                    Dialog.Show(KeeperPasswordDialog);
                }
            }
            else
            {
                IsKeeperVerificationSuccessful = false;
                KeeperVerificationStatus = "Keeper verification failed. Please check the code and try again.";
                IsControlEnabled = true;
                IsLoadingCircleVisible = false;
                Debug.WriteLine("Keeper verification failed. Please check the code and try again.");
            }
        }

        private void ValidatePasswordConfirmation()
        {
            // Convert SecureStrings to plain text (handle nulls)
            string pass = Password != null
                ? new NetworkCredential("", Password).Password
                : "";
          

            // Clear previous errors
            _errors.Remove("Password");
            _errors.Remove("ConfirmPassword");

            // Validate password length (only for main password)
            if (string.IsNullOrEmpty(pass) || pass.Length < 8)
            {
                _errors["Password"] = new List<string> { "Password must be at least 8 characters" };
            }

          

            // Notify UI for both fields
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs("Password"));

            // Securely clear temporary strings (optional but recommended)
            pass = string.Empty;
        }

  

        private async void LoginKeeper()
        {
            string password = new NetworkCredential("", Password).Password;
            if (VerifiedKeeper != null)
            {
                if (VerifiedKeeper?.VerifyPassword(password) == true)
                {
                    IsUserSessionActive = true;
                    IsMerchantSessionActive = false;
                    CurrentSessionStatus = "Active Session";
                    CurrentUserRole = "Keeper";
                    ApplicationState.Instance.DefaultAccount = new DefaultUserAccount(VerifiedKeeper);
                    ApplicationState.Instance.StoreMerchantAccount = CurrentMerchantAccount;
                    LoggedInUserAccountType = 0; // Keeper account type
                    // Stop the authorization timer if it was running
                    barcodeGenerator.StopAuthorizationTimer();
                    CurrentUserName = ApplicationState.Instance.DefaultAccount.FullName;

                    // Set current user store
                    if (CurrentStore != null)
                    {
                        ApplicationState.Instance.DefaultAccount.SetCurrentUserStore(CurrentStore);
                    }
                    else
                    {
                        CurrentStore = await _dataObjects.GetDefaultUserAccountStore();
                        ApplicationState.Instance.DefaultAccount.SetCurrentUserStore(CurrentStore);
                    }

                    OrdersViewModel _ordersViewModel = _services.GetRequiredService<OrdersViewModel>();
                    _ordersViewModel.SetLoggedInUserAccountType(LoggedInUserAccountType);                    
                    CurrentUserControl = _services.GetRequiredService<HomeView>();
                    Debug.WriteLine("Keeper session successfully activated");
                    UserLoginDialog.close();
                }
                else
                {
                    MessageBox.Show("Invalid password. Please try again.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                var keeper = await _dataObjects.FindKeeperByEmailAsync(KeeperEmail);
                if (keeper != null)
                {
                    if (keeper.VerifyPassword(password))
                    {
                        IsUserSessionActive = true;
                        IsMerchantSessionActive = false;
                        CurrentSessionStatus = "Active Session";
                        CurrentUserRole = "Keeper";
                                                
                        ApplicationState.Instance.DefaultAccount = new DefaultUserAccount(keeper);
                        ApplicationState.Instance.StoreMerchantAccount = CurrentMerchantAccount;
                        LoggedInUserAccountType = 0; // Keeper account type

                        // Set the current user store
                        if (CurrentStore != null)
                        {
                            ApplicationState.Instance.DefaultAccount.SetCurrentUserStore(CurrentStore);
                        }
                        else
                        {
                            CurrentStore = await _dataObjects.GetDefaultUserAccountStore();
                            ApplicationState.Instance.DefaultAccount.SetCurrentUserStore(CurrentStore);
                        }

                        // Stop the authorization timer if it was running
                        barcodeGenerator.StopAuthorizationTimer();
                        CurrentUserName = ApplicationState.Instance.DefaultAccount.FullName;
                        OrdersViewModel _ordersViewModel = _services.GetRequiredService<OrdersViewModel>();
                        _ordersViewModel.SetLoggedInUserAccountType(LoggedInUserAccountType);
                        CurrentUserControl = _services.GetRequiredService<HomeView>();
                        Debug.WriteLine("Keeper session successfully activated");
                        UserLoginDialog.close();
                    }
                    else
                    {
                        MessageBox.Show("Invalid password. Please try again.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Keeper not found. Please check your email and try again.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ShowSelectedView(object parameter)
        {
            _logger.Information("The parameter is: " + parameter.GetType().Name);
            if (parameter is string message)
            {
                _logger.Information("Selected view is: " + message);
                switch (message)
                {
                    case "HomeView":

                        ShowHomeView(); break;
                    case "EasyPosView":
                        ShowEasyPosView(); break;
                    case "ShopsView":
                        ShowShopsView();
                        break;
                    case "CreateShopView":
                        ShowCreateShopView();
                        break;
                    case "ShopKeepersView":
                        ShowShopKeepersView();
                        break;
                    case "OrdersView":
                        ShowOrdersView();
                        break;
                    case "InventoryView":
                        ShowInventoryView();
                        break;
                    case "PaymentsView":
                        ShowPaymentsView();
                        break;
                    case "PaymentAggregatorsView":
                        ShowPaymentAggregatorsView();
                        break;
                    case "ScheduledOrdersView":
                        ShowScheduledOrdersView();
                        break;
                    case "DiscountsView":
                        ShowDisountsView();
                        break;
                    case "PromotionsView":
                        ShowPromotionsView();
                        break;
                    case "SettingsView":
                        ShowSettingsView();
                        break;
                }
            }
        }

        private void ShowCreateMerchantAccountDialog()
        {
            CreateMerchantAccountDialog = new CreateMerchantAccountDialog();
            HandyControl.Controls.Dialog.Show(CreateMerchantAccountDialog);
        }

        private void ShowUserLoginDialog()
        {
            GenerateMerchantLoginQrCode();
            UserLoginDialog = new UserLoginDialog();
            HandyControl.Controls.Dialog.Show(UserLoginDialog);

        }


        private void ShowHomeView()
        {
            if (MerchantAccountExists())
            {

                if (IsUserSessionActive)
                {
                   
                        CurrentUserControl = _services.GetRequiredService<HomeView>(); 
                    
                  
                }
                else { 
                    ShowUserLoginDialog();
                }
            }
            else
            {
                IsMerchantAccountActivated = false;
                ShowCreateMerchantAccountDialog();

            }
        }

        private void ShowEasyPosView()
        {
            if (MerchantAccountExists())
            {
                Debug.WriteLine("Show easy pos view");                
                CurrentUserControl = _services.GetRequiredService<EasyPosView>();
                
            }
            else
            {
                IsMerchantAccountActivated = false;
                ShowCreateMerchantAccountDialog();
            }

        }

        private void ShowShopsView()
        {
            if (MerchantAccountExists())
            {
                ShopsViewModel shopsViewModel =new ShopsViewModel(_dataObjects);
                ShopsView shopsView = _services.GetRequiredService<ShopsView>();
                shopsView.addShopsViewModel(shopsViewModel);
                CurrentUserControl = shopsView;               
            }
            else
            {
                IsMerchantAccountActivated = false;
                ShowCreateMerchantAccountDialog();
            }
        }

        private void ShowCreateShopView()
        {
            if (MerchantAccountExists())
            {
                CurrentUserControl = _services.GetRequiredService<CreateShopView>();
               
            }
            else
            {
                IsMerchantAccountActivated = false;
                ShowCreateMerchantAccountDialog();
            }
        }

        private void ShowShopKeepersView()
        {
            if (MerchantAccountExists())
            {
                ShopKeepersViewModel keepersViewModel =new ShopKeepersViewModel(_dataObjects);
                ShopKeepersView shopKeepersView = _services.GetRequiredService<ShopKeepersView>();
                shopKeepersView.addKeeperViewModel(keepersViewModel);

                CurrentUserControl = shopKeepersView;
                

                if (!isUserSessionActive)
                {
                    ShowUserLoginDialog();
                }
                StoreHasKeeperAccounts();
            }
            else
            {
                IsMerchantAccountActivated = false;
                ShowCreateMerchantAccountDialog();
            }
        }

        private void ShowOrdersView()
        {
            if (MerchantAccountExists())
            {                
                CurrentUserControl = _services.GetRequiredService<OrdersView>();               
            }
            else
            {
                IsMerchantAccountActivated = true;
                ShowCreateMerchantAccountDialog();
            }
        }

        private void ShowInventoryView()
        {
            if (MerchantAccountExists())
            {    
                InventoryViewModel viewModel = _services.GetRequiredService<InventoryViewModel>();
                viewModel.SetLoggedInUserAccountType(LoggedInUserAccountType);
                CurrentUserControl = _services.GetRequiredService<InventoryView>();               
            }
            else
            {
                IsMerchantAccountActivated = false;
                ShowCreateMerchantAccountDialog();
            }
        }

        private void ShowPaymentsView()
        {
            if (MerchantAccountExists())
            {
                CurrentUserControl = _services.GetRequiredService<PaymentsView>();               
            }
            else
            {
                IsMerchantAccountActivated = false;
                ShowCreateMerchantAccountDialog();
            }
        }

        private void ShowPaymentAggregatorsView()
        {
            if (MerchantAccountExists())
            {
                CurrentUserControl = _services.GetRequiredService<PaymentAggregatorsView>();
            }
            else
            {
                IsMerchantAccountActivated = false;
                ShowCreateMerchantAccountDialog();
            }
        }

        private void ShowDisountsView()
        {
            if (MerchantAccountExists())
            {
                CurrentUserControl = _services.GetRequiredService<DiscountsView>();
             
            }
            else
            {
                IsMerchantAccountActivated = false;
                ShowCreateMerchantAccountDialog();
            }

        }

        private void ShowSettingsView()
        {
            if (MerchantAccountExists())
            {
                CurrentUserControl=_services.GetRequiredService<SettingsView>();
              
            }
            else
            {
                IsMerchantAccountActivated = false;
                ShowCreateMerchantAccountDialog();
            }
        }

        private void ShowPromotionsView()
        {
            if (MerchantAccountExists())
            {
                
                CurrentUserControl = _services.GetRequiredService<PromotionsView>();
              
            }
            else
            {
                IsMerchantAccountActivated = false;
                ShowCreateMerchantAccountDialog();
            }
        }

        private void ShowScheduledOrdersView()
        {
            if (MerchantAccountExists())
            {                
                CurrentUserControl = _services.GetRequiredService<ScheduledOrdersView>();
               
            }
            else
            {
                IsMerchantAccountActivated = false;
                ShowCreateMerchantAccountDialog();
            }
        }


        private void InitializeTimer()
        {
            _dispatchTimer = new DispatcherTimer();
            _dispatchTimer.Interval = TimeSpan.FromMicroseconds(1); // Update after every 1 second
            _dispatchTimer.Tick += (sender, e) => UpdateTime();
            _dispatchTimer.Start();
        }

        private void UpdateTime()
        {
            CurrentTime = DateTime.Now.ToString("dddd, MMMMM dd yyyy HH:mm:ss");
        }

        

        private bool MerchantAccountExists()
        {
            if (choziShopDatabaseManager.CheckDatabaseExists())
            {
                if (choziShopDatabaseManager.CheckTableExists() && choziShopDatabaseManager.MerchantAccountExists())
                {
                    return true;
                }
            }
            return false;
        }

        private bool StoreHasKeeperAccounts()
        {
            if (choziShopDatabaseManager.CheckDatabaseExists())
            {
                if (choziShopDatabaseManager.CheckTableExists() && choziShopDatabaseManager.StoreHasKeeperAccounts())
                {
                    _isStoreHavingKeeperAccounts = false;
                    return true;
                }
                else
                {
                    IsStoreHavingKeeperAccounts = true;
                }
            }

            return false;
        }

        private void OnSessionExpired()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                IsMerchantSessionActive = false;
                IsUserSessionActive = false;
                CurrentSessionStatus = "Inactive Session";
                CurrentUserRole = "Unknown User";
                _sessionManager.StopSessionMonitoring();
                barcodeGenerator.SetIsMerchantSessionActive(false);
                ApplicationState.Instance.DefaultAccount = null;    
                Debug.WriteLine("Merchant session expired");
                ShowUserLoginDialog();
            });
        }



        async Task<List<StoreResponse>> FetchMerchantStore()
        {
            if (SavedMerchant != null)
            {
                var baseApi = new BaseApi(savedMerchant.AuthToken);
                return await baseApi.GetWindowsAccountStores(SavedMerchant.OnlineMerchantId);
            }
            return new List<StoreResponse>();
        }

        async Task<List<CategorySectionResponse>> FetchStoreCategories(long onlineStoreId)
        {
            if (SavedMerchant != null)
            {
                var baseApi = new BaseApi(savedMerchant.AuthToken);
                return await baseApi.GetCategorySections(onlineStoreId);
            }
            return new List<CategorySectionResponse>();
        }

        private void ActivateMerchantSession(WindowsSessionResponse sessionResponse)
        {
            if (barcodeGenerator != null)
            {
                barcodeGenerator.ActivateMerchantSession(sessionResponse);
            }
        }

        private void UpdateConnectivityStatus(ConnectivityStatus status)
        {
            Application current = Application.Current;
            if (current != null)
            {
                current.Dispatcher.Invoke(() =>
                {
                    IsInternetConnected = status.IsInternetConnected;
                    IsXmppConnected = status.IsXmppConnected;

                });
            }
            ConnectivityStatus = status;
            if (IsInternetConnected)
                Debug.WriteLine("Internet is connected");
            else
                Debug.WriteLine($"Internet connected: {IsInternetConnected}");

            if (IsXmppConnected)
                Debug.WriteLine("Xmpp is connected. Can now send messages");
        }

        private void ValidateProperty(object value, [CallerMemberName] string propertyName = null)
        {
            if (propertyName == null) return;
            _errors.Remove(propertyName);

            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(this) { MemberName = propertyName };
            bool isValid = Validator.TryValidateProperty(value, context, validationResults);

            if (!isValid)
            {
                _errors[propertyName] = validationResults.Select(x => x.ErrorMessage).ToList();
            }


            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public bool HasErrors=> _errors.Any();
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        public IEnumerable GetErrors(string propertyName) => 
            _errors.TryGetValue(propertyName, out var errors) ? errors : Enumerable.Empty<string>();



        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }


    }
}
