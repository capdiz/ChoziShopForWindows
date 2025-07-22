using ChoziShop.Data.Models;
using ChoziShopForWindows.Data;
using ChoziShopForWindows.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.ViewModels
{
    public class PaymentsViewModel : INotifyPropertyChanged
    {
        private readonly IDataObjects dataObjects;
        private readonly IDialogService dialogService;

        private ObservableCollection<AirtelPayCollection> _airtelPayCollections;
        private string _airtelPayCollectionsHeader;

        public PaymentsViewModel(IDataObjects dataObjects, IDialogService dialogService)
        {
            this.dataObjects = dataObjects;
            this.dialogService = dialogService;
           _ = LoadAirtelPayCollections();
        }

        public string AirtelPayCollectionsHeader
        {
            get { return _airtelPayCollectionsHeader; }
            set
            {
                _airtelPayCollectionsHeader = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<AirtelPayCollection> AirtelPayCollections
        {
            get => _airtelPayCollections;
            set
            {
                if (_airtelPayCollections != value)
                {
                    _airtelPayCollections = value;
                    OnPropertyChanged();
                }
            }
        }

        private async Task LoadAirtelPayCollections()
        {
            var airtelPayCollections = await dataObjects
                .GetAirtelPayCollectionsAsync();

            if (airtelPayCollections.Count > 0)
            {
                var defaultUser = ApplicationState.Instance.DefaultAccount;
                _airtelPayCollections = new();
                foreach (var item in airtelPayCollections)
                {
                    var order = await dataObjects.GetOrderByOnlineId(item.CustomerOrderId);
                    if (order != null)
                    {
                        item.Order = order;
                        if (defaultUser.AccountType == 0 && order.OrderProcessedById == defaultUser.OnlineUserAccountId)
                        {
                            _airtelPayCollections.Add(item);
                        }
                        else
                        {
                            _airtelPayCollections.Add(item);
                        }
                    }
                }
                if (defaultUser.AccountType == 0)
                {
                    AirtelPayCollectionsHeader = $"{_airtelPayCollections.Count} Collections processed by you ({defaultUser.FullName}).";
                }
                else
                {
                    AirtelPayCollectionsHeader = $"{_airtelPayCollections.Count} Airtel Pay Collections";
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
