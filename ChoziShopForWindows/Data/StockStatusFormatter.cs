using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.Data
{
    public static class StockStatusFormatter
    {
        public static string FormatStockStatus(int outOfStockCount, int crticalOnStockCount, int lowOnStockCount)
        {
            var parts = new List<string>();
            if (outOfStockCount > 0)
            {
                parts.Add($"{outOfStockCount} out of stock");
            }

            if (crticalOnStockCount > 0)
            {
                parts.Add($"{crticalOnStockCount} critical");
            }

            if (lowOnStockCount > 0)
            {
                parts.Add($"{lowOnStockCount} low on stock");
            }

            return $"Stock status: {FormatSentenceParts(parts)}";
        }

        private static string FormatSentenceParts(List<string> parts)
        {
            switch (parts.Count)
            {
                case 1:
                    return parts[0];
                case 2: return $"{parts[0]} and {parts[1]}";
                default:
                    var allButLast = string.Join(", ", parts.Take(parts.Count - 1));
                    return $"{allButLast}, and {parts.Last()}";
            }
        }
    }
}
