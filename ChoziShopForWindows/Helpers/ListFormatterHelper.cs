using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.Helpers
{
    public static class ListFormatterHelper
    {
        public static string ToFriendlyString<T>(IEnumerable<T> items, Func<T, string> nameSelector)
        {
            if (items == null) return string.Empty;

            var validNames = items
                .Select(item => nameSelector(item))
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList();

            switch (validNames.Count)
            {
                case 0:
                    return string.Empty;
                case 1:
                    return validNames[0];
                case 2:
                    return $"{validNames[0]} and {validNames[1]}";
                default:
                    string firstPart = string.Join(", ", validNames.Take(validNames.Count - 1));
                    return $"{firstPart}, and {validNames.Last()}";
            }
        }

        public static string ToHyphenedString<T>(IEnumerable<T> items, Func<T, string> nameSelector)
        {
            return string.Join(
                "\n",
                items.Select(item => $"- {nameSelector(item)}")
            );
        }
    }
}
