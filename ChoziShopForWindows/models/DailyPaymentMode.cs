using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.models
{
    public class DailyPaymentMode
    {
        public string? PaymentMode { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal TotalPaymentAmount { get; set; }
    }
}
