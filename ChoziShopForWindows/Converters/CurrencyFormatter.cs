using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.Converters
{
    public static class CurrencyFormatter
    {
        public static string FormatToUgxCurrency(decimal amount)
        {
            var ugxCulture = (CultureInfo) CultureInfo.CurrentCulture.Clone();
            var nfi = ugxCulture.NumberFormat;

            // Configure currency format
            nfi.CurrencySymbol = " UGX";
            nfi.CurrencyDecimalDigits = 0;
            nfi.CurrencyDecimalSeparator = ".";
            nfi.CurrencyGroupSeparator = ",";


            // Pattern -> amount the currency e.g. 1,000,000 UGX
            nfi.CurrencyPositivePattern=2;
            // Pattern -> amount the currency e.g. -1,000,000 UGX
            nfi.CurrencyNegativePattern = 14;

            // Format the decimal amount as currency           
            return amount.ToString("C", nfi);
        }
    }
}
