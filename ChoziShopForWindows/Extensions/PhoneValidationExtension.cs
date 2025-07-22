using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ChoziShopForWindows.Extensions
{
    public static class PhoneValidationExtension
    {
        public static readonly DependencyProperty ValidatePhoneProperty =
            DependencyProperty.RegisterAttached("validatePhone", typeof(bool),
                typeof(PhoneValidationExtension), new PropertyMetadata(false, OnPhoneValidationChanged));         

        public static bool GetValidatePhone(TextBox textBox)=> (bool)textBox.GetValue(ValidatePhoneProperty);

        public static void SetValidatePhone(TextBox textBox, bool value) => textBox.SetValue(ValidatePhoneProperty, value);

        private static void OnPhoneValidationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                textBox.TextChanged -= TextBox_TextChanged;
                if ((bool)e.NewValue)
                {
                    textBox.TextChanged += TextBox_TextChanged;
                }
            }
        }

        private static void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;    
            var binding = textBox.GetBindingExpression(TextBox.TextProperty);
            binding?.ValidateWithoutUpdate();
        }
    }
}
