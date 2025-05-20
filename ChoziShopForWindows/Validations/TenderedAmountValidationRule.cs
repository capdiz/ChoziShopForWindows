using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ChoziShopForWindows.Validations
{
    public class TenderedAmountValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult(false, "Value cannot be empty.");
            }

            if (int.TryParse(value.ToString(), out _))
            {
                return ValidationResult.ValidResult;
            }

            return new ValidationResult(false, "Tendered amount isn't valid.");
        }
    }
}
