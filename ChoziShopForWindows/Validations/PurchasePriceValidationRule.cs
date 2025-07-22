using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ChoziShopForWindows.Validations
{
    public class PurchasePriceValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult(false, "Purchase price cannot be empty.");
            }
            if (decimal.TryParse(value.ToString(), out decimal purchasePrice) && purchasePrice >= 0)
            {
                return ValidationResult.ValidResult;
            }
            return new ValidationResult(false, "Purchase price must be greater than 0.");
        }
    }
   
}
