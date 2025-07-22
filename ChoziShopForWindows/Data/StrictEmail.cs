using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ChoziShopForWindows.Data
{
    public class StrictEmail : ValidationAttribute
    {
        private static readonly Regex _regex = new(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            if (value == null) return ValidationResult.Success;

            string email = value.ToString();

            if (string.IsNullOrWhiteSpace(email))
                return new ValidationResult("Email is required");

            return _regex.IsMatch(email)
                ? ValidationResult.Success
                : new ValidationResult("Invalid email format");
        }
    }
}
