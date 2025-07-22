using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.models
{
    public class Sale : INotifyPropertyChanged
    {
        private long _categorySectionId;
        public long CategorySectionId
        {
            get => _categorySectionId;
            set
            {
                if (_categorySectionId != value)
                {
                    _categorySectionId = value;
                    OnPropertyChanged();
                }
            }
        }

        private long _itemId;
        public long ItemId
        {
            get => _itemId;
            set
            {
                if (_itemId != value)
                {
                    _itemId = value;
                    OnPropertyChanged();
                }
            }
        }
        private int _saleNo;
        public int SaleNo
        {
            get => _saleNo;
            set
            {
                if (_saleNo != value)
                {
                    _saleNo = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _itemName;
        public string ItemName
        {
            get { return _itemName; }
            set
            {
                if (_itemName != value)
                {
                    _itemName = value;
                    OnPropertyChanged();
                }
            }
        }

        private decimal _unitPrice;
        public decimal UnitPrice
        {
            get { return _unitPrice; }
            set
            {
                if (_unitPrice != value)
                {
                    _unitPrice = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _unitsSold;
        public int UnitsSold
        {
            get { return _unitsSold; }
            set
            {
                if (_unitsSold != value)
                {
                    _unitsSold = value;
                    OnPropertyChanged();
                }
            }
        }

        private DateTime _saleDate;
        public DateTime SaleDate
        {
            get { return _saleDate; }
            set
            {
                if (_saleDate != value)
                {
                    _saleDate = value;
                    OnPropertyChanged();
                }
            }
        }

        private decimal _totalSalesAmount;
        public decimal TotalSalesAmount
        {
            get { return _totalSalesAmount; }
            set
            {
                if (_totalSalesAmount != value)
                {
                    _totalSalesAmount = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _salesTimeSpan;
        public string SalesTimeSpan
        {
            get { return _salesTimeSpan; }
            set
            {
                if (_salesTimeSpan != value)
                {
                    _salesTimeSpan = value;
                    OnPropertyChanged();
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
