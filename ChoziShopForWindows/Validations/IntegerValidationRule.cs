using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using ZXing.OneD;

namespace ChoziShopForWindows.Validations
{
    public class IntegerValidationRule : ValidationRule
    {
        public bool AllowNegative { get;set; }  =true;
        public bool AllowEmpty { get; set; } = false;

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string input = value as string;

            if (string.IsNullOrWhiteSpace(input))
            {
                return AllowEmpty ? ValidationResult.ValidResult
                    : new ValidationResult(false, "Field cannot be empty");
               
            }

            bool startsWithSign = input.StartsWith("-") || input.StartsWith("+");
            bool isValid = input.All(c=>char.IsDigit(c) ||
            (startsWithSign && input.IndexOf(c) == 0) && c == '-' && AllowNegative);
            if (!isValid)
                return new ValidationResult(false, "Only integer values are allowed");

            if (AllowNegative)
            {
                if(input.Length > 11) 
                    return new ValidationResult(false, "Number too large");
            }
            else
            {
                if (input.StartsWith("-"))
                    return new ValidationResult(false, "Negative values aren't allowed");

                if (input.Length > 10)
                    return new ValidationResult(false, "Number is beyond limit");
            }

            return ValidationResult.ValidResult;
        }
    }
}
