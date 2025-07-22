using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.models
{
    public class WeeklySale
    {
        public int WeekNumber { get; set; }
        public DateTime LastSaleDate { get; set; }  
        public string? ItemName { get; set; }
        public decimal TotalSalesAmount { get; set; }
    }
}
