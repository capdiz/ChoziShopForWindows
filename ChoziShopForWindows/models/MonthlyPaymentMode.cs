using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.models
{
    public class MonthlyPaymentMode
    {
        public string? PaymentMode { get; set; }
        public int Month {  get; set; } 
        public DateTime LastPaymentDate { get; set; }
        public decimal TotalPaymentAmount { get; set; }
    }
}
