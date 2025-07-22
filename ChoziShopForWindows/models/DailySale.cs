using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.models
{
    public class DailySale
    {
        public DateTime SaleDate { get; set; }
        public string? ItemName { get; set; }
        public decimal TotalSalesAmount { get; set; }
    }
}
