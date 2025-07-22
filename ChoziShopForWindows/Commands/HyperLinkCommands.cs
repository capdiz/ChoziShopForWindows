using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace ChoziShopForWindows.Commands
{
    public static class HyperLinkCommands
    {
        public static readonly DependencyProperty CommandProperty = 
            DependencyProperty.RegisterAttached("Command",
                typeof(ICommand),
                typeof(HyperLinkCommands),
                new PropertyMetadata(null, OnCommandChanged));

        public static ICommand GetCommand(DependencyObject obj) => (ICommand)obj.GetValue(CommandProperty);

        public static void SetCommand(DependencyObject obj, ICommand value) =>obj.SetValue(CommandProperty, value);

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Hyperlink hyperlink)
            {
                hyperlink.Click -= OnHyperLinkClicked;
                if (e.NewValue != null)
                {
                    hyperlink.Click += OnHyperLinkClicked;
                }
            }
        }

        private static void OnHyperLinkClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Hyperlink hyperlink && hyperlink.GetValue(CommandProperty) is ICommand command &&
                command.CanExecute(hyperlink.CommandParameter))
            {
                command.Execute(hyperlink.CommandParameter);
            }
        }
    }
}
