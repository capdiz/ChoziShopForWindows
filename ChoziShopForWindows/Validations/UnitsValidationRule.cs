using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ChoziShopForWindows.Validations
{
    public class UnitsValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult(false, "Units cannot be empty.");
            }
            if (int.TryParse(value.ToString(), out int units) && units > 0)
            {
                return ValidationResult.ValidResult;
            }
            return new ValidationResult(false, "Units must be greater than 0.");
        }
    }
   
}
