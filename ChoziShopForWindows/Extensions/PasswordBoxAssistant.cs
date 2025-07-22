using System.Net;
using System.Security;
using System.Windows;
using System.Windows.Controls;

namespace ChoziShopForWindows.Extensions
{
    public static class PasswordBoxAssistant
    {
        public static readonly DependencyProperty BindPasswordProperty =
            DependencyProperty.RegisterAttached("BindPassword", typeof(bool),
                typeof(PasswordBoxAssistant), new PropertyMetadata(false, OnBindPasswordChanged));

        public static readonly DependencyProperty BoundPasswordProperty =
            DependencyProperty.RegisterAttached("BoundPassword", typeof(SecureString),
                typeof(PasswordBoxAssistant), new FrameworkPropertyMetadata(null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,  // Critical fix
                    OnBoundPasswordChanged));

        public static readonly DependencyProperty UpdatingPasswordProperty =
            DependencyProperty.RegisterAttached("UpdatingPassword", typeof(bool),
                typeof(PasswordBoxAssistant), new PropertyMetadata(false));

        public static bool GetBindPassword(DependencyObject obj) =>
            (bool)obj.GetValue(BindPasswordProperty);

        public static void SetBindPassword(DependencyObject obj, bool value) =>
            obj.SetValue(BindPasswordProperty, value);

        public static SecureString GetBoundPassword(DependencyObject dp) =>
            (SecureString)dp.GetValue(BoundPasswordProperty);

        public static void SetBoundPassword(DependencyObject dp, SecureString value) =>
            dp.SetValue(BoundPasswordProperty, value);

        private static bool GetUpdatingPassword(DependencyObject dp) =>
            (bool)dp.GetValue(UpdatingPasswordProperty);

        private static void SetUpdatingPassword(DependencyObject dp, bool value) =>
            dp.SetValue(UpdatingPasswordProperty, value);

        private static void OnBindPasswordChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            if(dp is not PasswordBox passwordBox)
            {
                return;
            }

            
            passwordBox.PasswordChanged -= PasswordChanged;
            if ((bool)e.NewValue)
            {
                passwordBox.PasswordChanged += PasswordChanged;
            }
        }

        private static void OnBoundPasswordChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            if (dp is not PasswordBox passwordBox) return;
            if (!GetBindPassword(passwordBox)) return;
            if (GetUpdatingPassword(passwordBox)) return;

            passwordBox.PasswordChanged -= PasswordChanged;

            try
            {
                string newPassword = e.NewValue is SecureString secureString
                    ? new NetworkCredential(string.Empty, secureString).Password
                    : string.Empty;

                if (passwordBox.Password != newPassword)
                {
                    passwordBox.Password = newPassword;
                }
            }
            finally
            {
                passwordBox.PasswordChanged += PasswordChanged;
            }
        }

        private static void PasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = (PasswordBox)sender;
            if (!GetBindPassword(passwordBox)) return;

            SetUpdatingPassword(passwordBox, true);

            try
            {
                var securePassword = new SecureString();
                foreach (char c in passwordBox.Password)
                {
                    securePassword.AppendChar(c);
                }
                securePassword.MakeReadOnly();

                SetBoundPassword(passwordBox, securePassword);
            }
            finally
            {
                SetUpdatingPassword(passwordBox, false);
            }
        }
    }
}