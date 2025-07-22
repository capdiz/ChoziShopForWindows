using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.Serialized
{
    public class DefaultCategoryProduct
    {
        [JsonProperty("id")]
        public long Id { get; set; }
        [JsonProperty("product_name")]
        public required string ProductName { get; set; }
    }
}
