using ChoziShopForWindows.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ChoziShopForWindows.Converters
{
    public class PropertyCollectionConverter : IValueConverter
    {
        public string PropertyName { get; set; } = "Name";
        public string EmptyCollection { get; set; } = "No items available";
        public string Separator { get; set; } = ", ";
        public string OverflowFormat { get; set; } = "... ({0} more)";
        public int MaxItems { get; set; } = int.MaxValue;

        public object Convert(object value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            if(value==null)return EmptyCollection;
            if(!(value is IEnumerable collection)) return value.ToString();

            var items = new List<object>();
            foreach (var item in collection)
            {
                items.Add(item);
            }

            if(items.Count ==0) return EmptyCollection;

            // Extract property values from the collection
            var propertyValues = items
                .Select(item => item?.GetType().GetProperty(PropertyName)?
                    .GetValue(item, null)?.ToString() ?? string.Empty).ToList();

            if(propertyValues.Count>MaxItems)
            {
                return string.Join(Separator, propertyValues.Take(MaxItems)) +
                   string.Format(OverflowFormat, propertyValues.Count - MaxItems);
            }

            return ListFormatterHelper.ToFriendlyString(
                propertyValues,
                name => name
            );
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
