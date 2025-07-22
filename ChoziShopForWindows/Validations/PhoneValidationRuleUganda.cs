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
    public class PhoneValidationRuleUganda : ValidationRule
    {
        private static readonly PhoneNumberUtil PhoneUtil = PhoneNumberUtil.GetInstance();
        private const string DefaultRegion = "UG";
        private const int MinDigitsForValidation = 3;

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var phoneNumber = value as string;
            if (string.IsNullOrEmpty(phoneNumber))
            {
                return new ValidationResult(false, "A phone number is required");
            }

            // Skip validation for first digits being typed
            if(phoneNumber.Trim().Length < MinDigitsForValidation) 
                return ValidationResult.ValidResult;

            try
            {
                var number = PhoneUtil.Parse(phoneNumber, DefaultRegion);
                if (!PhoneUtil.IsPossibleNumber(number))
                    return new ValidationResult(false, "Invalid phone number format");

                if (!PhoneUtil.IsValidNumberForRegion(number, DefaultRegion))
                    return new ValidationResult(false, "Phone number isn't recognized on available networks in Uganda");

                return ValidationResult.ValidResult;
            }catch(NumberParseException ex)
            {
                return new ValidationResult(false, "Phone number is invalid");
            }
        }

    }
}
