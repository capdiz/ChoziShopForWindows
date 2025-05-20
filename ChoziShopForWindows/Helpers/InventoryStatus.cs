using ChoziShopForWindows.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.Helpers
{
    public static class InventoryStatus
    {
        public static (int min, int max) GetInventoryStatus(ProductInventoryStatus status)
        {
            return status switch
            {
                ProductInventoryStatus.Empty => (-10, 0),
                ProductInventoryStatus.Critical => (1, 5),
                ProductInventoryStatus.Low => (6, 10),
                ProductInventoryStatus.Normal => (10, 20),
                ProductInventoryStatus.High => (20, int.MaxValue),
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
            };
        }

        public static ProductInventoryStatus GetCategoryProductInventoryStatus(int remainingUnits)
        {
            switch (remainingUnits)
            {
                case int n when (n <= 0):
                    return ProductInventoryStatus.Empty;
                case int n when (n >= 1 && n <= 5):
                    return ProductInventoryStatus.Critical;
                case int n when (n >= 6 && n <= 10):
                    return ProductInventoryStatus.Low;
                case int n when (n >= 11 && n <= 20):
                    return ProductInventoryStatus.Normal;
                default:
                    return ProductInventoryStatus.High;
            }
        }
    }
}
