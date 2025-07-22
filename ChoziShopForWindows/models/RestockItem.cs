using ChoziShop.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.models
{
    public class RestockItem 
    {
        public int Id { get; set; }
        public string ItemName { get; set; }
        public decimal PurchasePrice { get; set; }
        public CategoryProduct CategoryProduct { get; set; }
    }
}
