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

namespace ChoziShopForWindows.ViewModels
{
    public class MainWindowViewModel : ObservableObject, INotifyPropertyChanged
    {
        private String _currentTime;
        private DispatcherTimer _dispatchTimer;
        private DatabaseManager choziShopDatabaseManager;

        private BitmapSource _qrCodeImage;

        private bool isMerchantAccountActivated = false;

        private string _selectedViewName = "HomeView";

        private LoginDialog LoginDialog;


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

        private int _storeSetupStatus = 0;
        private string storeSetupStatusText;


        private object _currentUserControl;

        private readonly ILogger _logger;


        private readonly IDataObjects _dataObjects;

        private BarcodeGenerator barcodeGenerator;

        private List<Store> _merchantStores;

        public MainWindowViewModel(IDataObjects dataObjects)
        {
            _logger = Log.ForContext<MainWindowViewModel>();

            InitializeTimer();

            _dataObjects = dataObjects;
            IsEasyPosviewVisible = Visibility.Collapsed;
            IsCreateShopViewVisible = Visibility.Collapsed;
            IsShopsViewVisible = Visibility.Collapsed;
            IsOrdersViewVisible = Visibility.Collapsed;
            IsShopKeepersViewVisible = Visibility.Collapsed;
            IsInventoryViewVisible = Visibility.Collapsed;
            IsPaymentsViewVisible = Visibility.Collapsed;
            IsScheduledOrdersViewVisible = Visibility.Collapsed;
            IsDiscountsViewVisible = Visibility.Collapsed;
            IsPromotionsViewVisible = Visibility.Collapsed;
            IsSettingsViewVisible = Visibility.Collapsed;
            IsProgressBarVisible = Visibility.Collapsed;


            ShowCurrentUserControlCommand = new Commands.RelayCommand(param => ShowSelectedView(param));
            GenerateCodeCommand = new Commands.RelayCommand(_ => GenerateCode());
            choziShopDatabaseManager = new DatabaseManager(DbFileConfig.FullDbOPath, "Merchants");
            IsMerchantAccountActivated = false;
            barcodeGenerator = new BarcodeGenerator();
            barcodeGenerator.MerchantResponseHandler += OnMerchantReceived;

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

        public string SelectedViewName
        {
            get { return _selectedViewName; }
            set
            {
                _selectedViewName = value;
                OnPropertyChanged();
            }
        }


        public String CurrentTime
        {
            get { return _currentTime; }
            set
            {
                _currentTime = value;
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
                base.OnPropertyChanged();
                Debug.WriteLine("CurrentUserControl updated to: " + value?.GetType().Name);
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


        public ICommand ShowCurrentUserControlCommand { get; }

        public ICommand GenerateCodeCommand { get; }

        public ICommand SaveMerchantDetailsCommand { get; }

        private void GenerateCode()
        {
            QrCodeImage = barcodeGenerator.GenerateBarcodeImage();
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
                        if (LoginDialog != null)
                        {
                            LoginDialog.close();
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
                                }
                                Debug.WriteLine(serializedStores.Count + " stores found");
                            }
                        }
                    });
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

        private void ShowLoginDialog()
        {
            LoginDialog = new LoginDialog();
            HandyControl.Controls.Dialog.Show(LoginDialog);
        }


        private void ShowHomeView()
        {
            if (MerchantAccountExists())
            {
                _logger.Information("User control is being shown.");
                IsHomeViewVisible = Visibility.Visible;

                IsEasyPosviewVisible = Visibility.Collapsed;
                IsCreateShopViewVisible = Visibility.Collapsed;
                IsShopsViewVisible = Visibility.Collapsed;
                IsOrdersViewVisible = Visibility.Collapsed;
                IsShopKeepersViewVisible = Visibility.Collapsed;
                IsInventoryViewVisible = Visibility.Collapsed;
                IsPaymentsViewVisible = Visibility.Collapsed;
                IsScheduledOrdersViewVisible = Visibility.Collapsed;
                IsDiscountsViewVisible = Visibility.Collapsed;
                IsPromotionsViewVisible = Visibility.Collapsed;
                IsSettingsViewVisible = Visibility.Collapsed;
            }
            else
            {
                IsMerchantAccountActivated = false;
                ShowLoginDialog();
            }
        }

        private void ShowEasyPosView()
        {
            if (MerchantAccountExists())
            {
                _logger.Information("Show easy pos view");
                IsEasyPosviewVisible = Visibility.Visible;

                IsHomeViewVisible = Visibility.Collapsed;
                IsCreateShopViewVisible = Visibility.Collapsed;
                IsShopsViewVisible = Visibility.Collapsed;
                IsOrdersViewVisible = Visibility.Collapsed;
                IsShopKeepersViewVisible = Visibility.Collapsed;
                IsInventoryViewVisible = Visibility.Collapsed;
                IsPaymentsViewVisible = Visibility.Collapsed;
                IsScheduledOrdersViewVisible = Visibility.Collapsed;
                IsDiscountsViewVisible = Visibility.Collapsed;
                IsPromotionsViewVisible = Visibility.Collapsed;
                IsSettingsViewVisible = Visibility.Collapsed;
            }
            else
            {
                IsMerchantAccountActivated = false;
                ShowLoginDialog();
            }

        }

        private void ShowShopsView()
        {
            if (MerchantAccountExists())
            {
                IsShopsViewVisible = Visibility.Visible;

                IsHomeViewVisible = Visibility.Collapsed;
                IsEasyPosviewVisible = Visibility.Collapsed;
                IsCreateShopViewVisible = Visibility.Collapsed;
                IsOrdersViewVisible = Visibility.Collapsed;
                IsShopKeepersViewVisible = Visibility.Collapsed;
                IsInventoryViewVisible = Visibility.Collapsed;
                IsPaymentsViewVisible = Visibility.Collapsed;
                IsScheduledOrdersViewVisible = Visibility.Collapsed;
                IsDiscountsViewVisible = Visibility.Collapsed;
                IsPromotionsViewVisible = Visibility.Collapsed;
                IsSettingsViewVisible = Visibility.Collapsed;
            }
            else
            {
                IsMerchantAccountActivated = false;
                ShowLoginDialog();
            }
        }

        private void ShowCreateShopView()
        {
            if (MerchantAccountExists())
            {
                IsCreateShopViewVisible = Visibility.Visible;

                IsHomeViewVisible = Visibility.Collapsed;
                IsEasyPosviewVisible = Visibility.Collapsed;
                IsShopsViewVisible = Visibility.Collapsed;
                IsOrdersViewVisible = Visibility.Collapsed;
                IsShopKeepersViewVisible = Visibility.Collapsed;
                IsInventoryViewVisible = Visibility.Collapsed;
                IsPaymentsViewVisible = Visibility.Collapsed;
                IsScheduledOrdersViewVisible = Visibility.Collapsed;
                IsDiscountsViewVisible = Visibility.Collapsed;
                IsPromotionsViewVisible = Visibility.Collapsed;
                IsSettingsViewVisible = Visibility.Collapsed;
            }
            else
            {
                IsMerchantAccountActivated = false;
                ShowLoginDialog();
            }
        }

        private void ShowShopKeepersView()
        {
            if (MerchantAccountExists())
            {
                IsShopKeepersViewVisible = Visibility.Visible;

                IsHomeViewVisible = Visibility.Collapsed;
                IsEasyPosviewVisible = Visibility.Collapsed;
                IsShopsViewVisible = Visibility.Collapsed;
                IsOrdersViewVisible = Visibility.Collapsed;
                IsCreateShopViewVisible = Visibility.Collapsed;
                IsInventoryViewVisible = Visibility.Collapsed;
                IsPaymentsViewVisible = Visibility.Collapsed;
                IsScheduledOrdersViewVisible = Visibility.Collapsed;
                IsDiscountsViewVisible = Visibility.Collapsed;
                IsPromotionsViewVisible = Visibility.Collapsed;
                IsSettingsViewVisible = Visibility.Collapsed;
            }
            else
            {
                IsMerchantAccountActivated = false;
                ShowLoginDialog();
            }
        }

        private void ShowOrdersView()
        {
            if (MerchantAccountExists())
            {
                IsOrdersViewVisible = Visibility.Visible;

                IsHomeViewVisible = Visibility.Collapsed;
                IsEasyPosviewVisible = Visibility.Collapsed;
                IsShopsViewVisible = Visibility.Collapsed;
                IsCreateShopViewVisible = Visibility.Collapsed;
                IsInventoryViewVisible = Visibility.Collapsed;
                IsShopKeepersViewVisible = Visibility.Collapsed;
                IsPaymentsViewVisible = Visibility.Collapsed;
                IsScheduledOrdersViewVisible = Visibility.Collapsed;
                IsDiscountsViewVisible = Visibility.Collapsed;
                IsPromotionsViewVisible = Visibility.Collapsed;
                IsSettingsViewVisible = Visibility.Collapsed;
            }
            else
            {
                IsMerchantAccountActivated = true;
                ShowLoginDialog();
                BarcodeGenerator barcodeGenerator = new BarcodeGenerator();
                QrCodeImage = barcodeGenerator.GenerateBarcodeImage();
            }
        }

        private void ShowInventoryView()
        {
            if (MerchantAccountExists())
            {
                IsInventoryViewVisible = Visibility.Visible;

                IsHomeViewVisible = Visibility.Collapsed;
                IsEasyPosviewVisible = Visibility.Collapsed;
                IsOrdersViewVisible = Visibility.Collapsed;
                IsShopsViewVisible = Visibility.Collapsed;
                IsCreateShopViewVisible = Visibility.Collapsed;
                IsPaymentsViewVisible = Visibility.Collapsed;
                IsShopKeepersViewVisible = Visibility.Collapsed;
                IsScheduledOrdersViewVisible = Visibility.Collapsed;
                IsDiscountsViewVisible = Visibility.Collapsed;
                IsPromotionsViewVisible = Visibility.Collapsed;
                IsSettingsViewVisible = Visibility.Collapsed;
            }
            else
            {
                IsMerchantAccountActivated = false;
                ShowLoginDialog();
            }
        }

        private void ShowPaymentsView()
        {
            if (!MerchantAccountExists())
            {
                IsPaymentsViewVisible = Visibility.Visible;

                IsHomeViewVisible = Visibility.Collapsed;
                IsEasyPosviewVisible = Visibility.Collapsed;
                IsCreateShopViewVisible = Visibility.Collapsed;
                IsShopsViewVisible = Visibility.Collapsed;
                IsOrdersViewVisible = Visibility.Collapsed;
                IsShopKeepersViewVisible = Visibility.Collapsed;
                IsInventoryViewVisible = Visibility.Collapsed;
                IsScheduledOrdersViewVisible = Visibility.Collapsed;
                IsDiscountsViewVisible = Visibility.Collapsed;
                IsPromotionsViewVisible = Visibility.Collapsed;

                IsSettingsViewVisible = Visibility.Collapsed;
            }
            else
            {
                IsMerchantAccountActivated = true;
                ShowLoginDialog();
            }
        }

        private void ShowDisountsView()
        {
            if (MerchantAccountExists())
            {
                IsDiscountsViewVisible = Visibility.Visible;

                IsHomeViewVisible = Visibility.Collapsed;
                IsEasyPosviewVisible = Visibility.Collapsed;
                IsCreateShopViewVisible = Visibility.Collapsed;
                IsShopsViewVisible = Visibility.Collapsed;
                IsOrdersViewVisible = Visibility.Collapsed;
                IsShopKeepersViewVisible = Visibility.Collapsed;
                IsInventoryViewVisible = Visibility.Collapsed;
                IsPaymentsViewVisible = Visibility.Collapsed;
                IsScheduledOrdersViewVisible = Visibility.Collapsed;
                IsPromotionsViewVisible = Visibility.Collapsed;
                IsSettingsViewVisible = Visibility.Collapsed;
            }
            else
            {
                IsMerchantAccountActivated = false;
                ShowLoginDialog();
            }

        }

        private void ShowSettingsView()
        {
            if (MerchantAccountExists())
            {
                IsSettingsViewVisible = Visibility.Visible;

                IsHomeViewVisible = Visibility.Collapsed;
                IsEasyPosviewVisible = Visibility.Collapsed;
                IsCreateShopViewVisible = Visibility.Collapsed;
                IsShopsViewVisible = Visibility.Collapsed;
                IsOrdersViewVisible = Visibility.Collapsed;
                IsShopKeepersViewVisible = Visibility.Collapsed;
                IsInventoryViewVisible = Visibility.Collapsed;
                IsPaymentsViewVisible = Visibility.Collapsed;
                IsScheduledOrdersViewVisible = Visibility.Collapsed;
                IsDiscountsViewVisible = Visibility.Collapsed;
                IsPromotionsViewVisible = Visibility.Collapsed;
            }
            else
            {
                IsMerchantAccountActivated = false;
                ShowLoginDialog();
            }
        }

        private void ShowPromotionsView()
        {
            if (MerchantAccountExists())
            {
                IsPromotionsViewVisible = Visibility.Visible;

                IsHomeViewVisible = Visibility.Collapsed;
                IsEasyPosviewVisible = Visibility.Collapsed;
                IsCreateShopViewVisible = Visibility.Collapsed;
                IsShopsViewVisible = Visibility.Collapsed;
                IsOrdersViewVisible = Visibility.Collapsed;
                IsShopKeepersViewVisible = Visibility.Collapsed;
                IsInventoryViewVisible = Visibility.Collapsed;
                IsPaymentsViewVisible = Visibility.Collapsed;
                IsScheduledOrdersViewVisible = Visibility.Collapsed;
                IsDiscountsViewVisible = Visibility.Collapsed;
                IsSettingsViewVisible = Visibility.Collapsed;
            }
            else
            {
                IsMerchantAccountActivated = false;
                ShowLoginDialog();
            }
        }

        private void ShowScheduledOrdersView()
        {
            if (MerchantAccountExists())
            {
                IsScheduledOrdersViewVisible = Visibility.Visible;

                IsHomeViewVisible = Visibility.Collapsed;
                IsEasyPosviewVisible = Visibility.Collapsed;
                IsCreateShopViewVisible = Visibility.Collapsed;
                IsShopsViewVisible = Visibility.Collapsed;
                IsOrdersViewVisible = Visibility.Collapsed;
                IsShopKeepersViewVisible = Visibility.Collapsed;
                IsInventoryViewVisible = Visibility.Collapsed;
                IsPaymentsViewVisible = Visibility.Collapsed;
                IsDiscountsViewVisible = Visibility.Collapsed;
                IsPromotionsViewVisible = Visibility.Collapsed;
                IsSettingsViewVisible = Visibility.Collapsed;
            }
            else
            {
                IsMerchantAccountActivated = false;
                ShowLoginDialog();
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

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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



        async Task<List<StoreResponse>> FetchMerchantStore()
        {
            if (SavedMerchant != null)
            {
                var baseApi = new BaseApi(savedMerchant.AuthToken);
                return await baseApi.GetWindowsAccountStores(SavedMerchant.OnlineMerchantId);
            }
            return new List<StoreResponse>();
        }
    }
}
