using ChoziShop.Data.Models;
using ChoziShopForWindows.Enums;
using ChoziShopForWindows.models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.Helpers
{
    public static class SalesHelper
    {       
        public static IEnumerable<Sale> GroupSalesByOrderList(IEnumerable<Order> orders, GroupByScope groupByScope)
        {
            var sales = orders.Where(x=>x.OrderStatus ==3).SelectMany(o =>
            o.OrderProductItems.Select(item => item.OnlineCategoryProductId)
            .Distinct()
            .Select(productId => new Sale
            {
                ItemId = productId,
                ItemName = o.OrderProductItems.FirstOrDefault(item => item.OnlineCategoryProductId == productId)?.ProductName ?? "Unknown",
                UnitPrice = o.OrderProductItems.FirstOrDefault(item => item.OnlineCategoryProductId == productId)?.UnitCost ?? 0,
                UnitsSold = o.OrderProductItems.Where(cp => cp.OnlineCategoryProductId == productId).Count(),
                TotalSalesAmount = o.OrderProductItems.FirstOrDefault(item => item.OnlineCategoryProductId == productId)?.UnitCost * o.OrderProductItems.Where(cp => cp.OnlineCategoryProductId == productId).Count() ?? 0,
                SaleDate = o.CreatedAt.Date,

            }))
                .ToList();


            switch (groupByScope)
            {
                case GroupByScope.Day:
                    var todaySales = sales.GroupBy(s => new
                    {
                        s.ItemId,
                        s.ItemName,
                        s.UnitPrice,
                        s.UnitsSold,
                        s.TotalSalesAmount,
                        s.SaleDate.Date
                    })
                        .Select(g => new Sale
                        {
                            ItemId = g.Key.ItemId,
                            ItemName = g.Key.ItemName,
                            UnitPrice = g.Key.UnitPrice,
                            UnitsSold = g.Sum(s => s.UnitsSold),
                            TotalSalesAmount = g.Sum(s => s.TotalSalesAmount),
                            SalesTimeSpan = ItemSaleTimeSpan(orders, g.Key.ItemId)
                        });                    
                    return todaySales;
                case GroupByScope.Week:
                    return sales.GroupBy(s => new
                    {
                        s.ItemId,
                        s.ItemName,
                        s.UnitPrice,
                        s.UnitsSold,
                        s.TotalSalesAmount,                        
                        Week = (s.SaleDate.Day - 1) / 7 + 1
                    })                    
                    .Select(g => new Sale
                    {
                        ItemId = g.Key.ItemId,
                        ItemName = g.Key.ItemName,
                        UnitPrice = g.First().UnitPrice,
                        UnitsSold = g.Sum(s => s.UnitsSold),
                        TotalSalesAmount = g.Sum(s => s.TotalSalesAmount),
                        SalesTimeSpan = ItemSaleTimeSpan(orders, g.Key.ItemId)
                    });
                case GroupByScope.Month:
                    return sales.GroupBy(s => new
                    {
                        s.ItemId,
                        s.ItemName,
                        s.UnitPrice,
                        s.UnitsSold,
                        s.TotalSalesAmount,
                        s.SaleDate.Month,                        
                    })
                        .Select(g => new Sale
                        {
                            ItemId = g.Key.ItemId,
                            ItemName = g.First().ItemName,
                            UnitPrice = g.First().UnitPrice,
                            UnitsSold = g.Sum(s => s.UnitsSold),
                            TotalSalesAmount = g.Sum(s=> s.TotalSalesAmount),
                            SalesTimeSpan = MonthlySaleTimeSpan(orders, g.Key.ItemId)
                        });
                default:
                    return sales;
            }

        }

        public static IEnumerable<DailySale> DailySales(IEnumerable<Order> orders)
        {
            var sales = orders.SelectMany(o =>
          o.OrderProductItems.Select(item => item.OnlineCategoryProductId)
          .Distinct()
          .Select(productId => new Sale
          {
              ItemId = productId,
              ItemName = o.OrderProductItems.FirstOrDefault(item => item.OnlineCategoryProductId == productId)?.ProductName ?? "Unknown",
              UnitPrice = o.OrderProductItems.FirstOrDefault(item => item.OnlineCategoryProductId == productId)?.UnitCost ?? 0,
              UnitsSold = o.OrderProductItems.Where(cp => cp.OnlineCategoryProductId == productId).Count(),
              TotalSalesAmount = o.OrderProductItems.FirstOrDefault(item => item.OnlineCategoryProductId == productId)?.UnitCost * o.OrderProductItems.Where(cp => cp.OnlineCategoryProductId == productId).Count() ?? 0,
              SaleDate = o.CreatedAt.Date,

          }))
              .ToList();
            var dailySales = sales.GroupBy(s => new
            {
                
                s.SaleDate
            })
                .Select(g => new DailySale
                {                    
                    SaleDate = g.Key.SaleDate,
                    TotalSalesAmount = g.Sum(s => s.TotalSalesAmount)
                });
            return dailySales;
        }

        public static IEnumerable<WeeklySale> WeeklySales(IEnumerable<Order> orders)
        {
            var sales = orders.SelectMany(o =>
          o.OrderProductItems.Select(item => item.OnlineCategoryProductId)
          .Distinct()
          .Select(productId => new Sale
          {
              ItemId = productId,
              ItemName = o.OrderProductItems.FirstOrDefault(item => item.OnlineCategoryProductId == productId)?.ProductName ?? "Unknown",
              UnitPrice = o.OrderProductItems.FirstOrDefault(item => item.OnlineCategoryProductId == productId)?.UnitCost ?? 0,
              UnitsSold = o.OrderProductItems.Where(cp => cp.OnlineCategoryProductId == productId).Count(),
              TotalSalesAmount = o.OrderProductItems.FirstOrDefault(item => item.OnlineCategoryProductId == productId)?.UnitCost * o.OrderProductItems.Where(cp => cp.OnlineCategoryProductId == productId).Count() ?? 0,
              SaleDate = o.CreatedAt.Date,
          }))
              .ToList();
            var weeklySales = sales.GroupBy(s => new
            {
               
                LastSaleDate = s.SaleDate,
                Week = (s.SaleDate.Day - 1) / 7 + 1
            })
                .Select(g => new WeeklySale
                {
                    WeekNumber = g.Key.Week,
                    LastSaleDate = g.Key.LastSaleDate,
                    TotalSalesAmount = g.Sum(s => s.TotalSalesAmount)
                });
            return weeklySales;
        }

        public static IEnumerable<MonthlySale> MonthlySales(IEnumerable<Order> orders)
        {
            var sales = orders.SelectMany(o =>
          o.OrderProductItems.Select(item => item.OnlineCategoryProductId)
          .Distinct()
          .Select(productId => new Sale
          {
              ItemId = productId,
              ItemName = o.OrderProductItems.FirstOrDefault(item => item.OnlineCategoryProductId == productId)?.ProductName ?? "Unknown",
              UnitPrice = o.OrderProductItems.FirstOrDefault(item => item.OnlineCategoryProductId == productId)?.UnitCost ?? 0,
              UnitsSold = o.OrderProductItems.Where(cp => cp.OnlineCategoryProductId == productId).Count(),
              TotalSalesAmount = o.OrderProductItems.FirstOrDefault(item => item.OnlineCategoryProductId == productId)?.UnitCost * o.OrderProductItems.Where(cp => cp.OnlineCategoryProductId == productId).Count() ?? 0,
              SaleDate = o.CreatedAt.Date,
          }))
              .ToList();
            var monthlySales = sales.GroupBy(s => new
            {
                
                LastSaledate = new DateTime(s.SaleDate.Year, s.SaleDate.Month, 1),
                s.SaleDate.Month
            })
                .Select(g => new MonthlySale
                {
                    MonthNumber = g.Key.Month,
                    LastSaleDate =g.Key.LastSaledate,
                    TotalSalesAmount = g.Sum(s => s.TotalSalesAmount)
                });
            return monthlySales;
        }

        private static DateTime GetWeekStart(DateTime datetTime)
        {
            var diff = datetTime.DayOfWeek - DayOfWeek.Sunday;
            if (diff < 0)
            {
                diff += 7;
            }
            return datetTime.AddDays(-diff).Date;

        }

        private static string ItemSaleTimeSpan(IEnumerable<Order> orders, long itemId)
        {
            // Get timestamps for sale item name
            var itemSales = orders
                .Where(o => o.OrderProductItems.Any(item => item.OnlineCategoryProductId == itemId))
                .Select(o => o.CreatedAt)
                .OrderByDescending(d => d)
                .Take(1)
                .OrderBy(d => d)
                .ToList();
       
            return FriendlyDateHelper.LastSaleTimespan(itemSales[0], DateTime.Now);
        }

        private static string MonthlySaleTimeSpan(IEnumerable<Order> orders, long itemId)
        {
            // Get timestamps for sale item name
            var itemSales = orders
                .Where(o => o.OrderProductItems.Any(item => item.OnlineCategoryProductId == itemId))
                .Select(o => o.CreatedAt)
                .OrderByDescending(d => d)
                .Take(1)
                .OrderBy(d => d)
                .ToList();

            return $"Last sale at: {itemSales[0].ToString()}";
        }

    }

}
