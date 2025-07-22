using ChoziShopForWindows.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.models
{
    public class StockItem : INotifyPropertyChanged
    {
        private int _itemNo;
        public int ItemNo
        {
            get { return _itemNo; }
            set
            {
                _itemNo = value;
                OnPropertyChanged();
            }
        }   

        private int _id;
        public int Id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }

        private string _itemName;
        public string ItemName
        {
            get { return _itemName; }
            set
            {
                _itemName = value;
                OnPropertyChanged();
            }
        }

        private int _quantity;
        public int Quantity
        {
            get { return _quantity; }
            set
            {
                _quantity = value; 
                OnPropertyChanged();
            }
        }

        private ProductInventoryStatus _status;
        public ProductInventoryStatus Status
        {
            get { return _status; }
            set
            {
                _status = value; 
                OnPropertyChanged();

            }
        }

        public string InventoryStatusDescription
        {
            get
            {
                return Status switch
                {
                    ProductInventoryStatus.Empty => "OutOfStock",
                    ProductInventoryStatus.Critical => "Critical",
                    ProductInventoryStatus.Low => "Low",
                    ProductInventoryStatus.Normal => "Normal",
                    ProductInventoryStatus.High => "High",
                    _ => "Unknown"
                };
            }
        }

        public string RestockItemHeader
        {
            get
            {
                return Status switch
                {
                    ProductInventoryStatus.Empty => $"Restock {ItemName} (ID: {Id})",
                    ProductInventoryStatus.Critical => $"Topup {ItemName} (ID: {Id})",
                    ProductInventoryStatus.Low => $"Topup {ItemName} (ID: {Id})",
                    _ => $"Restock {ItemName} (ID: {Id})"

                };

            }
        }

        public string DiscontinueItemHeader
        {
            get
            {
                return $"Discontinue {ItemName} (ID: {Id})";
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
