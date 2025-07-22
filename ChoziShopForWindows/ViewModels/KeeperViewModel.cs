using ChoziShop.Data.Models;
using ChoziShopForWindows.Data;
using ChoziShopForWindows.Validations;
using ChoziShopForWindows.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ChoziShopForWindows.ViewModels
{
    public class KeeperViewModel : ObservableObject, INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private readonly IDataObjects _dataObjects;
        private readonly Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();

        private Keeper _newlyVerifiedKeeper;
        private SecureString _password;
        private SecureString _confirmPassword;

        private AddKeeperPasswordDialog _keeperPasswordDialog;

        private string _verifiedKeeperheader;

        public KeeperViewModel(IDataObjects dataObjects, Keeper newlyVerifiedKeeper)
        {
            _dataObjects = dataObjects;
            NewlyVerifiedKeeper = newlyVerifiedKeeper;
            VerifiedKeeperHeader = $"Hello {newlyVerifiedKeeper.FullName}, please create your password to continue";
            CreateKeeperPasswordCommand = new Commands.RelayCommand(async _ => await CreateKeeperPasswordAsync());
        }

        public KeeperViewModel(IDataObjects dataObjects)
        {
            _dataObjects = dataObjects;
        }

        public void SetKeeperPasswordDialog(AddKeeperPasswordDialog keeperPasswordDialog)
        {
            _keeperPasswordDialog = keeperPasswordDialog;
        }

        public Keeper NewlyVerifiedKeeper
        {
            get { return _newlyVerifiedKeeper; }
            set
            {
                _newlyVerifiedKeeper = value;
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

        public SecureString ConfirmPassword
        {
            get { return _confirmPassword; }
            set
            {
                _confirmPassword = value;
                OnPropertyChanged();
                ValidatePasswordConfirmation();
            }
        }

        public string VerifiedKeeperHeader
        {
            get { return _verifiedKeeperheader; }
            set
            {
                _verifiedKeeperheader = value;
                OnPropertyChanged();
            }
        }

        public ICommand CreateKeeperPasswordCommand { get; }

        private async Task CreateKeeperPasswordAsync()
        {
            if (CanVerifyKeeperAccount())
            {
                string password = new NetworkCredential("", Password).Password;

                NewlyVerifiedKeeper.SetPassword(password);
                // clear password from memory
                password = string.Empty;
                Password.Dispose();
                ConfirmPassword.Dispose();

                var updatedKeeper = await _dataObjects.Keepers.UpdateAndReturnEntityAsync(NewlyVerifiedKeeper);
                if (updatedKeeper != null)
                {
                    MessageBox.Show("Password has successfully been updated. Login to start using ChoziShop");
                    // Close KeeperPasswordDialog and open login dialog for keeper to login with their new password
                    _keeperPasswordDialog.close();
                }
            }
            else
            {
                MessageBox.Show("Please ensure that the passwords match and are at least 8 characters long.");
            }
        }

        private bool CanVerifyKeeperAccount()
        {
            string pass1 = new NetworkCredential("", Password).Password;
            string pass2 = new NetworkCredential("password", ConfirmPassword).Password;
            bool isValid = pass1 == pass2 && pass1.Length > 0;

            // clear 
            pass1 = string.Empty;
            pass2 = string.Empty;
            return isValid;
        }

        private void ValidatePasswordConfirmation()
        {
            // Convert secure strings to plain text
            string password = Password != null ? new NetworkCredential(string.Empty, Password).Password : string.Empty;
            string confirmPass = ConfirmPassword != null ? new NetworkCredential(string.Empty, ConfirmPassword).Password : string.Empty;

            // clear previous errors
            _errors.Remove("Password");
            _errors.Remove("ConfirmPassword");

            // Validate password length (for the main password)
            if (string.IsNullOrEmpty(password) || password.Length < 8)
            {
                _errors["Password"] = new List<string> { "Password must be at least 8 characters long." };
            }

            // Validate password confirmation match only when confirmation pass isn't empty
            if (!string.IsNullOrEmpty(confirmPass) && password != confirmPass)
            {
                _errors["ConfirmPassword"] = new List<string> { "Passwords do not match." };
            }

            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs("Password"));
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs("ConfirmPassword"));

            // clear memory
            password = string.Empty;
            confirmPass = string.Empty;
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        public bool HasErrors => _errors.Any();
        public IEnumerable GetErrors(string propertyName) =>
           _errors.TryGetValue(propertyName, out var errors) ? errors : Enumerable.Empty<string>();

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
