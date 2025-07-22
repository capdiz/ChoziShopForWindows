using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.models
{
    public class WeeklyPaymentMode
    {
        public string? PaymentMode { get; set; }
        public int Week { get; set; }
        public DateTime LastPaymentDate { get; set; }
        public decimal TotalPaymentAmount { get; set; }
    }
}
