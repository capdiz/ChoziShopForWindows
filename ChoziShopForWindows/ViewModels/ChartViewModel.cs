using ChoziShopForWindows.Helpers;
using ChoziShopForWindows.models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using LiveCharts;

using System.Windows.Input;
using LiveCharts.Definitions.Series;
using ChoziShopForWindows.Data;
using LiveCharts.Wpf;

using LiveCharts.Defaults;
using System.Drawing;

using Brushes = System.Windows.Media.Brushes;
using System.Diagnostics;
using ChoziShop.Data.Models;
using ChoziShopForWindows.Enums;
using System.Windows;
using System.Windows.Media;
using ChoziShopForWindows.Converters;

namespace ChoziShopForWindows.ViewModels
{
    public class ChartViewModel : INotifyPropertyChanged
    {
        private string _title;
        private List<Order> _todaysOrders = new();
        private List<Order> _weeklyOrders = new();
        private List<Order> _monthlyOrders = new();

        private SeriesCollection _series;
        private List<string> _labels;

        private bool _isSalesChartVisible = false;
        
        public Func<double, string> Formatter { get; set; }
        private string[] palettes = new[]
            {
               "#4e79a7", "#f28e2c", "#e15759", "#76b7b2", "#59a14f",
               "#edc949", "#af7aa1", "#ff9da7", "#9c755f", "#bab0ac"
           };

       
        public ChartViewModel(IDataObjects dataObjects)
        {

            Task.Run(async () => TodaysOrders = await dataObjects.GetTodaysOrdersAsync()).Wait();
            Task.Run(async () => WeeklyOrders = await dataObjects.GetCurrentWeekEntitiesAsync()).Wait();
            Task.Run(async () => MonthlyOrders = await dataObjects.GetCurrentMonthEntitiesAsync()).Wait();
            GenerateDailySalesCommand = new Commands.RelayCommand(async _ => await GenerateDailySalesChart());
            GenerateWeeklySalesCommand = new Commands.RelayCommand(async _ => await GenerateWeeklySalesChart());
            GenerateMonthlySalesCommand = new Commands.RelayCommand(async _ => await GenerateMonthlySalesChart());

        }

        public string Title
        {
            get { return _title; }
            set { _title = value;
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

        public List<Order> TodaysOrders
        {
            get
            {
                return _todaysOrders;
            }
            set
            {
                _todaysOrders = value;
                OnPropertyChanged();
            }
        }

        public List<Order> WeeklyOrders
        {
            get
            {
                return _weeklyOrders;
            }
            set
            {
                _weeklyOrders = value;
                OnPropertyChanged();
            }
        }

        public List<Order> MonthlyOrders
        {
            get
            {
                return _monthlyOrders;
            }
            set
            {
                _monthlyOrders = value;
                OnPropertyChanged();
            }
        }

        public bool IsSalesChartVisible
        {
            get { return _isSalesChartVisible; }
            set
            {
                _isSalesChartVisible = value;
                OnPropertyChanged();
            }
        }


        public ICommand GenerateDailySalesCommand { get; set; }
        public ICommand GenerateWeeklySalesCommand { get; set; }
        public ICommand GenerateMonthlySalesCommand { get; set; }

        private async Task GenerateDailySalesChart()
        {
            var palette = palettes.Select(hex =>
            {
                var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hex);
                return new SolidColorBrush(color);
            }).ToArray();

            Application.Current.Dispatcher.Invoke(() =>
            {
                // Add new series for today's orders  
                if (TodaysOrders != null && TodaysOrders.Count > 0)
                {
                    var sales = SalesHelper.GroupSalesByOrderList(TodaysOrders, GroupByScope.Day);
                    var newSeries = new SeriesCollection();
                    var values = new ChartValues<DateTimePoint>();
                    var labels = new List<string>();
                    var datapoints = GetDataPoints(GroupByScope.Day);
                    foreach (var point in datapoints.OrderBy(p => p.Key))
                    {
                        values.Add(new DateTimePoint(point.Key, point.Value));
                        labels.Add(FormatLabel(point.Key, GroupByScope.Day));
                        Debug.WriteLine($"Key: {point.Key}, Value: {point.Value}");
                        Debug.WriteLine($"Formatted Label: {FormatLabel(point.Key, GroupByScope.Day)}");
                    }

                    foreach (var sale in sales.Select((s, i) => new { s, i }))
                    {
                        var columnSerie = new ColumnSeries
                        {
                            Title = sale.s.ItemName,
                            Values = new ChartValues<double> { (double)sale.s.TotalSalesAmount },
                            Stroke = null,
                            Fill = palette[sale.i % palettes.Length],
                            MaxColumnWidth = 30,
                            DataLabels = true,
                            FontSize = 10,
                            //LabelPoint = point => $"{CurrencyFormatter.FormatToUgxCurrency(sale.s.TotalSalesAmount)}",
                        };
                        newSeries.Add(columnSerie);
                    }

                    Series = newSeries;
                    Labels = labels;
                    IsSalesChartVisible = true;
                }
            });
        }

        private async Task GenerateWeeklySalesChart()
        {
            var palette = palettes.Select(hex =>
            {
                var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hex);
                return new SolidColorBrush(color);
            }).ToArray();

            Application.Current.Dispatcher.Invoke(() =>
            {
                // Add new series for today's orders
                if (WeeklyOrders != null && WeeklyOrders.Count > 0)
                {
                    var sales = SalesHelper.GroupSalesByOrderList(WeeklyOrders, GroupByScope.Week);
                    var newSeries = new SeriesCollection();
                    var values = new ChartValues<DateTimePoint>();
                    var labels = new List<string>();
                    var datapoints = GetDataPoints(GroupByScope.Week);
                    foreach (var point in datapoints.OrderBy(p => p.Key))
                    {
                        values.Add(new DateTimePoint(point.Key, point.Value));
                        labels.Add(FormatLabel(point.Key, GroupByScope.Week));
                        Debug.WriteLine($"Key: {point.Key}, Value: {point.Value}");
                        Debug.WriteLine($"Formatted Label: {FormatLabel(point.Key, GroupByScope.Week)}");
                    }

                    foreach (var sale in sales.Select((s, i) => new { s, i }))
                    {
                        var columnSerie = new ColumnSeries
                        {
                            Title = sale.s.ItemName,
                            Values = new ChartValues<double> { (double)sale.s.TotalSalesAmount },
                            Stroke = null,
                            Fill = palette[sale.i % palettes.Length],
                            MaxColumnWidth = 30,
                            DataLabels = true,
                            FontSize = 10,
                            //LabelPoint = point => $"{CurrencyFormatter.FormatToUgxCurrency(sale.s.TotalSalesAmount)}",
                        };
                        newSeries.Add(columnSerie);
                    }

                    Series = newSeries;
                    Labels = labels;
                    IsSalesChartVisible = true;
                }
            });
        }

        private async Task GenerateMonthlySalesChart()
        {
            var palette = palettes.Select(hex =>
            {
                var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hex);
                return new SolidColorBrush(color);
            }).ToArray();
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Add new series for today's orders
                if (MonthlyOrders != null && MonthlyOrders.Count > 0)
                {
                    var sales = SalesHelper.GroupSalesByOrderList(MonthlyOrders, GroupByScope.Month);
                    var newSeries = new SeriesCollection();
                    var values = new ChartValues<DateTimePoint>();
                    var labels = new List<string>();
                    var datapoints = GetDataPoints(GroupByScope.Month);
                    foreach (var point in datapoints.OrderBy(p => p.Key))
                    {
                        values.Add(new DateTimePoint(point.Key, point.Value));
                        labels.Add(FormatLabel(point.Key, GroupByScope.Month));
                        Debug.WriteLine($"Key: {point.Key}, Value: {point.Value}");
                        Debug.WriteLine($"Formatted Label: {FormatLabel(point.Key, GroupByScope.Month)}");
                    }

                    foreach (var sale in sales.Select((s, i) => new { s, i }))
                    {
                        var columnSerie = new ColumnSeries
                        {
                            Title = sale.s.ItemName,
                            Values = new ChartValues<double> { (double)sale.s.TotalSalesAmount },
                            Stroke = null,
                            Fill = palette[sale.i % palettes.Length],
                            MaxColumnWidth = 30,
                            DataLabels = true,
                            FontSize = 10,
                            //LabelPoint = point => $"{CurrencyFormatter.FormatToUgxCurrency(sale.s.TotalSalesAmount)}",
                        };
                        newSeries.Add(columnSerie);
                    }

                    Series = newSeries;
                    Labels = labels;
                    IsSalesChartVisible = true;
                }
            });
        }

        private Dictionary<DateTime, double> GetDataPoints(GroupByScope scope)
        {
            switch (scope)
            {
                case GroupByScope.Week:
                    var weeklySales = SalesHelper.WeeklySales(WeeklyOrders);
                    return weeklySales.ToDictionary(
                        sale => sale.LastSaleDate,
                        sale => (double)sale.TotalSalesAmount);
                case GroupByScope.Month:
                    var monthlySales = SalesHelper.MonthlySales(MonthlyOrders);
                    return monthlySales.ToDictionary(
                        sale => sale.LastSaleDate,
                        sale => (double)sale.TotalSalesAmount
                        );
                default:
                    var dailySales = SalesHelper.DailySales(TodaysOrders);
                    return dailySales.ToDictionary(
                        sale => sale.SaleDate,
                        sale => (double)sale.TotalSalesAmount
                        );
            }
        }

        private string FormatLabel(DateTime date, GroupByScope scope)
        {
            Debug.WriteLine($"Formatting label for date: {date}, scope: {scope}");
            return scope switch
            {
                GroupByScope.Day => date.ToString("dd MMM yy"),
                GroupByScope.Week => $"Week {(date.Day-1) / 7 + 1} of {date:MMM/yy}",
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
