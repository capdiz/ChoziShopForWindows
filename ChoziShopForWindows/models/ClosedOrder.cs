using ChoziShop.Data.Models;
using ChoziShopForWindows.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.models
{
    public class ClosedOrder : INotifyPropertyChanged
    {

        public ClosedOrder(Order order)
        {
            ClosedOrderId = order.Id;
            OrderItems = ListFormatterHelper.ToFriendlyString(order.OrderProductItems, item => item.ProductName);
            OrderItemsCount = order.OrderProductItems.Count;
            TotalOrderAmount = order.TotalAmountCents;
            SavedOrder = order;
        }

        private Order _savedOrder;
        public Order SavedOrder
        {
            get { return _savedOrder; }
            set
            {
                _savedOrder = value;
                OnPropertyChanged();
            }
        }

        private int _itemNo;
        public int ItemNo
        {
            get
            {
                return _itemNo;
            }
            set
            {
                _itemNo = value;
                OnPropertyChanged();
            }
        }

        private int _closedOrderId;
        public int ClosedOrderId
        {
            get => _closedOrderId;
            set
            {
                if (_closedOrderId != value)
                {
                    _closedOrderId = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _orderItems = string.Empty;
        public string OrderItems
        {
            get => _orderItems;
            set
            {
                if (_orderItems != value)
                {
                    _orderItems = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _orderItemsCount;
        public int OrderItemsCount
        {
            get => _orderItemsCount;
            set
            {
                if (_orderItemsCount != value)
                {
                    _orderItemsCount = value;
                    OnPropertyChanged();
                }
            }
        }

        private decimal _totalOrderAmount;
        public decimal TotalOrderAmount
        {
            get => _totalOrderAmount;
            set
            {
                if (_totalOrderAmount != value)
                {
                    _totalOrderAmount = value;
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


