using ChoziShop.Data.Models;
using ChoziShopForWindows.Converters;
using ChoziShopForWindows.Data;
using ChoziShopForWindows.Enums;
using ChoziShopForWindows.Helpers;
using ChoziShopForWindows.models;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
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
using System.Windows.Media;

namespace ChoziShopForWindows.ViewModels
{
    public class MobilePaymentsChartViewModel : INotifyPropertyChanged
    {
        private List<Order> _todaysOrders;
        private List<Order> _weeklyOrders;
        private List<Order> _monthlyOrders;

        private List<DailyPaymentMode> _dailyPaymentModes;
        private List<WeeklyPaymentMode> _weeklyPaymentModes;
        private List<MonthlyPaymentMode> _monthlyPaymentModes;


        private bool _isPaymentModesChartVisible = false;

        private SeriesCollection _series;
        private List<string> _labels;

        public Func<double, string> Formatter { get; set; }
      

        public MobilePaymentsChartViewModel(IDataObjects dataObjects) {
            IsPaymentModesChartVisible = false;
            Task.Run(async () => TodaysOrders = await dataObjects.GetTodaysOrdersAsync()).Wait();
            Task.Run(async () => WeeklyOrders = await dataObjects.GetCurrentWeekEntitiesAsync()).Wait();
            Task.Run(async () => MonthlyOrders = await dataObjects.GetCurrentMonthEntitiesAsync()).Wait();

            ShowDailyPaymentsChartCommand = new Commands.RelayCommand(async _ => await GenerateDailyPaymentsChartAsync());
            ShowWeeklyPaymentsChartCommand =new Commands.RelayCommand(async _ => await GenerateWeeklyPaymentsChartAsync());
            ShowMonthlyPaymentsChartCommand =new Commands.RelayCommand(async _ => await GenerateMonthlyPaymentsChartAsync());
        }
        
       public List<Order> TodaysOrders
        {
            get { return _todaysOrders; }
            set
            {
                _todaysOrders = value;
                OnPropertyChanged();
            }
        }

        public List<Order> WeeklyOrders
        {
            get { return _weeklyOrders; }
            set
            {
                _weeklyOrders = value;
                OnPropertyChanged();
            }
        }

        public List<Order> MonthlyOrders
        {
            get { return _monthlyOrders; }
            set
            {
                _monthlyOrders = value;
                OnPropertyChanged();
            }
        }

        public bool IsPaymentModesChartVisible
        {
            get { return _isPaymentModesChartVisible; }
            set
            {
                _isPaymentModesChartVisible = value;
                OnPropertyChanged();
            }
        }

        public SeriesCollection Series
        {
            get { return _series; }
            set
            {
                _series = value;
                OnPropertyChanged();
            }
        }

        public List<string> Labels
        {
            get { return _labels; }
            set
            {
                _labels = value;
                OnPropertyChanged();
            }
        }

        public ICommand ShowDailyPaymentsChartCommand { get; }
        public ICommand ShowWeeklyPaymentsChartCommand { get; }
        public ICommand ShowMonthlyPaymentsChartCommand { get; }

        private async Task GenerateDailyPaymentsChartAsync()
        {
        

            Application.Current.Dispatcher.Invoke(() =>
            {
                // Add new series for today's orders  
                if (TodaysOrders != null && TodaysOrders.Count > 0)
                {

                    var newSeries = new SeriesCollection();
                    var values = new ChartValues<DateTimePoint>();
                    var labels = new List<string>();
                    var datapoints = GetDataPoints(TodaysOrders, GroupByScope.Day);
                    foreach (var point in datapoints.OrderBy(p => p.Key))
                    {
                        values.Add(new DateTimePoint(point.Key, point.Value));
                        labels.Add(FormatLabel(point.Key, GroupByScope.Day));
                        Debug.WriteLine($"Key: {point.Key}, Value: {point.Value}");
                        Debug.WriteLine($"Formatted Label: {FormatLabel(point.Key, GroupByScope.Day)}");
                    }

                    foreach (var paymentMode in _dailyPaymentModes.Select((p, i) => new { p, i }))
                    {
                        var columnSerie = new ColumnSeries
                        {
                            Title = paymentMode.p.PaymentMode,
                            Values = new ChartValues<double> { (double) paymentMode.p.TotalPaymentAmount },
                            Stroke = null,
                            Fill = paymentMode.p.PaymentMode == "Cash" ? Brushes.Green : Brushes.DarkBlue,
                            MaxColumnWidth = 30,
                            DataLabels = true,
                            FontSize = 10,
                            LabelPoint = point => $"{CurrencyFormatter.FormatToUgxCurrency(paymentMode.p.TotalPaymentAmount)}",
                        };
                        newSeries.Add(columnSerie);
                    }

                    Series = newSeries;
                    Labels = labels;
                    IsPaymentModesChartVisible = true;
                }
            });
        }

        private async Task GenerateWeeklyPaymentsChartAsync()
        {


            Application.Current.Dispatcher.Invoke(() =>
            {
                // Add new series for today's orders  
                if (WeeklyOrders != null && WeeklyOrders.Count > 0)
                {

                    var newSeries = new SeriesCollection();
                    var values = new ChartValues<DateTimePoint>();
                    var labels = new List<string>();
                    var datapoints = GetDataPoints(WeeklyOrders, GroupByScope.Week);
                    foreach (var point in datapoints.OrderBy(p => p.Key))
                    {
                        values.Add(new DateTimePoint(point.Key, point.Value));
                        labels.Add(FormatLabel(point.Key, GroupByScope.Week));
                        Debug.WriteLine($"Key: {point.Key}, Value: {point.Value}");
                        Debug.WriteLine($"Formatted Label: {FormatLabel(point.Key, GroupByScope.Week)}");
                    }

                    foreach (var paymentMode in _weeklyPaymentModes.Select((p, i) => new { p, i }))
                    {
                        var columnSerie = new ColumnSeries
                        {
                            Title = paymentMode.p.PaymentMode,
                            Values = new ChartValues<double> { (double)paymentMode.p.TotalPaymentAmount },
                            Stroke = null,
                            Fill = paymentMode.p.PaymentMode == "Cash" ? Brushes.Green : Brushes.DarkBlue,
                            MaxColumnWidth = 30,
                            DataLabels = true,
                            FontSize = 10,
                            LabelPoint = point => $"{CurrencyFormatter.FormatToUgxCurrency(paymentMode.p.TotalPaymentAmount)}",
                        };
                        newSeries.Add(columnSerie);
                    }

                    Series = newSeries;
                    Labels = labels;
                    IsPaymentModesChartVisible = true;
                }
            });
        }

        private async Task GenerateMonthlyPaymentsChartAsync()
        {


            Application.Current.Dispatcher.Invoke(() =>
            {
                // Add new series for today's orders  
                if (MonthlyOrders != null && MonthlyOrders.Count > 0)
                {

                    var newSeries = new SeriesCollection();
                    var values = new ChartValues<DateTimePoint>();
                    var labels = new List<string>();
                    var datapoints = GetDataPoints(MonthlyOrders, GroupByScope.Month);
                    foreach (var point in datapoints.OrderBy(p => p.Key))
                    {
                        values.Add(new DateTimePoint(point.Key, point.Value));
                        labels.Add(FormatLabel(point.Key, GroupByScope.Month));
                        Debug.WriteLine($"Key: {point.Key}, Value: {point.Value}");
                        Debug.WriteLine($"Formatted Label: {FormatLabel(point.Key, GroupByScope.Month)}");
                    }

                    foreach (var paymentMode in _monthlyPaymentModes.Select((p, i) => new { p, i }))
                    {
                        var columnSerie = new ColumnSeries
                        {
                            Title = paymentMode.p.PaymentMode,
                            Values = new ChartValues<double> { (double)paymentMode.p.TotalPaymentAmount },
                            Stroke = null,
                            Fill = paymentMode.p.PaymentMode == "Cash" ? Brushes.Green : Brushes.DarkBlue,
                            MaxColumnWidth = 30,
                            DataLabels = true,
                            FontSize = 10,
                            LabelPoint = point => $"{CurrencyFormatter.FormatToUgxCurrency(paymentMode.p.TotalPaymentAmount)}",
                        };
                        newSeries.Add(columnSerie);
                    }

                    Series = newSeries;
                    Labels = labels;
                    IsPaymentModesChartVisible = true;
                }
            });
        }

        private Dictionary<DateTime, double> GetDataPoints(List<Order> sortedOrders, GroupByScope scope)
        {
            switch (scope)
            {
                case GroupByScope.Week:
                    _weeklyPaymentModes = sortedOrders.GroupBy(s => new
                    {
                        PaymentMode = s.PreferredPaymentMode,
                        Week = (s.CreatedAt.Day - 1) / 7 + 1,
                    }).Select(g => new WeeklyPaymentMode
                    {
                        PaymentMode = g.Key.PaymentMode == 0 ? "Cash" : "Mobile Money",
                        Week = g.Key.Week,
                        LastPaymentDate = g.Min(order => order.CreatedAt.Date),
                        TotalPaymentAmount = g.Sum(order => order.TotalAmountCents)
                    }).ToList();
                    Debug.WriteLine("Total: "+sortedOrders.Sum(x => x.TotalAmountCents));
                    foreach(WeeklyPaymentMode mode in _weeklyPaymentModes)
                    {
                        Debug.WriteLine("no of payment modes: " + _weeklyPaymentModes.Count);
                        Debug.WriteLine(mode.PaymentMode);
                    }
                    return _weeklyPaymentModes.ToDictionary(
                        mode => mode.LastPaymentDate,
                        mode => (double)mode.TotalPaymentAmount
                        );
                case GroupByScope.Month:
                    _monthlyPaymentModes = sortedOrders.GroupBy(order => new
                    {
                        PreferredPaymentMode = order.PreferredPaymentMode,
                        CreatedAt = new DateTime(order.CreatedAt.Year, order.CreatedAt.Month, 1),
                        Month = order.CreatedAt.Month
                    }).Select(g => new MonthlyPaymentMode
                    {
                        PaymentMode = g.Key.PreferredPaymentMode == 0 ? "Cash" : "Mobile Money",
                        Month = g.Key.Month,
                        LastPaymentDate = g.Key.CreatedAt,
                        TotalPaymentAmount = g.Sum(order => order.TotalAmountCents)
                    }).ToList();
                    return _monthlyPaymentModes.ToDictionary(
                        mode => mode.LastPaymentDate,
                        mode => (double)mode.TotalPaymentAmount
                        );
                default:
                    _dailyPaymentModes = sortedOrders.GroupBy(order => new
                    {
                        PaymentMode = order.PreferredPaymentMode,
                        PaymentDate = order.CreatedAt.Date
                    }).Select(g => new DailyPaymentMode
                    {
                        PaymentMode = g.Key.PaymentMode == 0 ? "Cash" : "Mobile Money",
                        PaymentDate = g.Key.PaymentDate,
                        TotalPaymentAmount = g.Sum(order => order.TotalAmountCents)
                    }).ToList();
                    return _dailyPaymentModes.ToDictionary(
                        mode => mode.PaymentDate,
                        mode => (double)mode.TotalPaymentAmount
                        );
            }
        }

        private string FormatLabel(DateTime date, GroupByScope scope)
        {
            Debug.WriteLine($"Formatting label for date: {date}, scope: {scope}");
            return scope switch
            {
                GroupByScope.Day => date.ToString("dd MMM yy"),
                GroupByScope.Week => $"Week {(date.Day - 1) / 7 + 1} of {date:MMM/yy}",
                GroupByScope.Month => date.ToString("MMM yyyy"),
                _ => date.ToString("dd MMM")
            };
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
