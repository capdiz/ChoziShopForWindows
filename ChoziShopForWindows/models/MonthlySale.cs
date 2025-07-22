using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.models
{
    public class MonthlySale
    {
        public int MonthNumber { get; set; }
        public DateTime LastSaleDate { get; set; }
        public string? ItemName { get; set; }
        public decimal TotalSalesAmount { get; set; }
        
    }
}
