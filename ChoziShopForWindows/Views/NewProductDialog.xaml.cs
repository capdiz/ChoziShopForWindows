using ChoziShopForWindows.ViewModels;
using HandyControl.Controls;
using HandyControl.Interactivity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChoziShopForWindows.Views
{
    /// <summary>
    /// Interaction logic for NewProductDialog.xaml
    /// </summary>
    public partial class NewProductDialog : Border
    {
        public NewProductDialog(InventoryViewModel inventoryViewModel)
        {
            InitializeComponent();
            DataContext = inventoryViewModel;
        }

        public void close()
        {
            ControlCommands.Close.Execute(true, this);
        }

        public void FocusListBox()
        {
            if (ProductNamesListBox.Items.Count > 0)
            {
                ProductNamesListBox.Focus();
                Keyboard.Focus(ProductNamesListBox);
                ProductNamesListBox.SelectedIndex = 0; // Select the first item
            }
        }

        public void FocusTextBox()
        {
            txtProductName.Focus();
            Keyboard.Focus(txtProductName);
        }

        private void SuggestionsPopup_Opened(object sender, EventArgs e)
        {
            FocusListBox();
        }

        private void txtProductName_GotFocus(object sender, RoutedEventArgs e)
        {
           if(!string.IsNullOrEmpty(txtProductName.Text))
            {
                var vm = (InventoryViewModel)DataContext;
                vm.IsPopupOpen = true;
            }
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var vm = (InventoryViewModel)DataContext;
            if (vm.IsPopupOpen)
            {
                switch (e.Key)
                {
                    case Key.Down:
                        if (ProductNamesListBox.Items.Count > 0)
                        {
                            FocusListBox();
                            e.Handled = true;
                        }
                        break;
                    case Key.Enter:
                        vm.ApplySuggestion();
                        FocusTextBox();
                        e.Handled = true;
                        break;
                }
            }
            else
            {
                if (e.Key == Key.Down && vm.ProductNames.Any())
                {
                    vm.IsPopupOpen = true;
                    e.Handled = true;
                }
            }
        }

        private void ProductNamesListBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var vm = (InventoryViewModel)DataContext;

            switch (e.Key)
            {
                case Key.Enter:
                    vm.ApplySuggestion();
                    FocusTextBox();
                    e.Handled = true;
                    break;
                case Key.Escape:
                    vm.IsPopupOpen = false;
                    FocusTextBox();
                    e.Handled = true;
                    break;
                case Key.Up:
                    if (ProductNamesListBox.SelectedIndex == 0)
                    {
                        FocusTextBox();
                        e.Handled = true;
                    }
                    break;
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            var vm = (InventoryViewModel)DataContext;
            if (!vm.IsPopupOpen) return;
            Debug.WriteLine($"Hello {e.Key}");
            switch (e.Key)
            {
                case Key.Down:
                    vm.SelectNext();
                    e.Handled = true;
                    break;
                case Key.Up:
                    vm.SelectPrevious();
                    e.Handled = true;
                    break;
                case Key.Enter:
                    vm.ApplySuggestion();
                    e.Handled = true;
                    break;
                case Key.Escape:
                    vm.IsPopupOpen = false;
                    e.Handled = true;
                    break;
            }
        }
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            txtProductName.Background = Brushes.LightYellow;
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            txtProductName.Background = Brushes.White;
        }
        private void ListBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var vm = (InventoryViewModel)DataContext;
            vm.ApplySuggestion();
        }
    }
}
