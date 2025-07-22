using ChoziShop.Data.Models;
using ChoziShopForWindows.Data;
using ChoziShopForWindows.MerchantsApi;
using ChoziShopForWindows.Services;
using ChoziShopForWindows.Views;
using HandyControl.Controls;
using HandyControl.Tools.Extension;
using PhoneNumbers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MessageBox = System.Windows.MessageBox;

namespace ChoziShopForWindows.ViewModels
{
    public class ShopKeepersViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private readonly IDataObjects _dataObjects;
        private readonly Dictionary<string, List<string>> _errors = new();

        private string _fullName;
        private string _email;
        private int _phoneNumber;
        private Keeper _keeper;        
        private List<Keeper> _storeKeepers;
        
        private AddKeeperDialog _addKeeperDialog;

        private bool _isNewKeeperControlVisible;
        private bool _isStoreKeepersDatagridVisible;
        private bool _isSendSmsCodeButtonEnabled;

        public ShopKeepersViewModel(IDataObjects dataObjects) {
            _dataObjects = dataObjects;
            LoadStoreKeepers();
            IsStoreKeepersDatagridVisible = false;            
            SendInvitationCodeCommand = new Commands.RelayCommand(_ => SendInvitationCodeSms( PhoneNumber.ToString()));
            SaveKeeperCommand =new Commands.RelayCommand(_=> SaveStoreKeeper() );
        }

        public Keeper CurrentKeeper
        {
            get { return _keeper; }
            set
            {
                _keeper = value;
                OnPropertyChanged();
            }
        }

        public string FullName
        {
            get { return _fullName; }
            set
            {
                _fullName = value;
                CurrentKeeper.FullName = value;
                OnPropertyChanged();
            }
        }

        [Required(ErrorMessage ="Your shop keeper's email is required")]
        [StrictEmail(ErrorMessage = "Invalid email address")]
        public string Email
        {
            get { return _email; }
            set
            {
                _email = value;
                ValidateProperty(value);
                CurrentKeeper.Email = value;
                OnPropertyChanged();
            }
        }

     

        public int PhoneNumber
        {
            get { return _phoneNumber; }
            set
            {
                _phoneNumber = value;
                OnPropertyChanged();
            }
        }

       public bool IsNewKeeperControlVisible
        {
            get { return _isNewKeeperControlVisible; }
            set
            {
                _isNewKeeperControlVisible = value;
                OnPropertyChanged();
            }
        }

        public bool IsStoreKeepersDatagridVisible
        {
            get { return _isStoreKeepersDatagridVisible; }
            set
            {
                _isStoreKeepersDatagridVisible=value;
                OnPropertyChanged();    
            }
        }

        public bool IsSendSmsCodeButtonEnabled
        {
            get { return _isSendSmsCodeButtonEnabled; }
            set
            {
                _isSendSmsCodeButtonEnabled = value;
                OnPropertyChanged();
            }
        }

        public List<Keeper> StoreKeepers
        {
            get { return _storeKeepers; }
            set
            {
                _storeKeepers = value;
                OnPropertyChanged();
            }
        }

        public AddKeeperDialog KeeperDialog
        {
            get { return _addKeeperDialog; }
            set
            {
                _addKeeperDialog = value;
                OnPropertyChanged();
            }
        }

        public ICommand SendInvitationCodeCommand { get; }
        public ICommand SaveKeeperCommand { get; }

        private async void LoadStoreKeepers()
        {
            var currentStore = ApplicationState.Instance.CurrentStore;
            if (currentStore != null)
            {
                StoreKeepers = await _dataObjects.Keepers.GetAllById((int)currentStore.OnlineStoreId);
                IsNewKeeperControlVisible = StoreKeepers.Count == 0;
                IsSendSmsCodeButtonEnabled = StoreKeepers.Count == 0;
                IsStoreKeepersDatagridVisible = StoreKeepers.Count > 0;
                Debug.WriteLine("Store keepers "+StoreKeepers.Count + " Current Store: "+currentStore.OnlineStoreId);
            }
        }

        private async void SendInvitationCodeSms(string phoneNumber)
        {

            if (IsValidPhoneNumber(phoneNumber))
            {
                IsSendSmsCodeButtonEnabled = false;
                AddKeeperDialog dialog = new AddKeeperDialog(this);
                KeeperDialog = dialog;
                var defaultUserAccount = ApplicationState.Instance.DefaultAccount;
                var currentStore = ApplicationState.Instance.CurrentStore;
                if (defaultUserAccount != null && currentStore != null)
                {
                    _keeper = new Keeper
                    {
                        PhoneNumber = phoneNumber,
                        OnlineStoreId = currentStore.OnlineStoreId
                    };
                    BaseApi baseApi = new BaseApi(defaultUserAccount.AuthToken);
                    var keeperAccount =
                        await baseApi.CreateKeeperAccount(defaultUserAccount.OnlineUserAccountId, currentStore.OnlineStoreId, _keeper);
                    if (keeperAccount != null)
                    {
                        _keeper.OnlineKeeperId = keeperAccount.Id;
                        _keeper.AuthToken="Not Applicable";
                        _keeper.FullJid = keeperAccount.FullJid;
                        _keeper.BareJid = keeperAccount.BareJid;
                        _keeper.CreatedAt = keeperAccount.CreatedAt;
                        _keeper.UpdatedAt = keeperAccount.UpdatedAt;

                        CurrentKeeper = _keeper;
                        Dialog.Show(KeeperDialog);
                    }
                }
            }
            else
            {
                MessageBox.Show("Invalid phone number");
            }
        }

        private bool IsValidPhoneNumber(string phoneNumber)
        {
            try
            {
                PhoneNumberUtil phoneUtil = PhoneNumberUtil.GetInstance();
                var number = phoneUtil.Parse(phoneNumber, "UG");
                if (!phoneUtil.IsPossibleNumber(number))
                    return false;

                if (!phoneUtil.IsValidNumberForRegion(number, "UG"))
                    return false;

                return true;
            }
            catch (NumberParseException ex)
            {
                return false;
            }
        }

        private async Task SaveStoreKeeper()
        {
            if (!string.IsNullOrEmpty(FullName) && !string.IsNullOrEmpty(Email))
            {
                var savedKeeper = await _dataObjects.Keepers.SaveAndReturnEntityAsync(CurrentKeeper);
                IsNewKeeperControlVisible = savedKeeper != null;
                if (IsNewKeeperControlVisible && savedKeeper != null)
                {
                    StoreKeepers.Add(savedKeeper);
                    IsNewKeeperControlVisible = false;
                    IsStoreKeepersDatagridVisible = true;
                    MessageBox.Show("Keeper successfully added to your ChoziShop", "Keeper Saved Successfully",
                          MessageBoxButton.OK, MessageBoxImage.Information);
                    KeeperDialog.close();
                }
            }
            else
            {
                MessageBox.Show("Make sure all fields have been provided. Shop keeper's name and email are required", "Missing Field Detected!",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ValidateProperty(object value, [CallerMemberName] string propertyName = null)
        {
            if (propertyName == null) return;
            _errors.Remove(propertyName);

            var validationResults =new List<ValidationResult>();
            var context =new ValidationContext(this) { MemberName = propertyName };
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

        public event PropertyChangedEventHandler? PropertyChanged;
        

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
