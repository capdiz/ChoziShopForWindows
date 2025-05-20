using ChoziShop.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.Helpers
{
    public static class DataListHelper
    {
        public static void ReplaceOrAdd<T>(List<T> list, Func<T, bool> predicate, T newItem)
        {
            int index = list.FindIndex(new Predicate<T>(predicate));
            if (index != -1)
            {
                list[index] = newItem;
            }
            else
            {
                list.Add(newItem);
            }
        }

    }
}
