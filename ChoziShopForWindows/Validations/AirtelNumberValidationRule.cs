using PhoneNumbers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ChoziShopForWindows.Validations
{
    public class AirtelNumberValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string phoneNumber = value?.ToString();
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return new ValidationResult(false, "An Airtel phone number is required.");

            try
            {
                PhoneNumberUtil phoneNumberUtil = PhoneNumberUtil.GetInstance();
                PhoneNumber parsedNumber = phoneNumberUtil.Parse(phoneNumber, "UG");
                if (!phoneNumberUtil.IsValidNumber(parsedNumber))
                    return new ValidationResult(false, "Invalid Airtel phone number.");
            }
            catch (NumberParseException)
            {
                return new ValidationResult(false, "Invalid Airtel phone number.");
            }
            return ValidationResult.ValidResult;
        }
    }
}
