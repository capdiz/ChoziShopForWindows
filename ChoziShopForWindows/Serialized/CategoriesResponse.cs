using ChoziShop.Data.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.Serialized
{
    public class CategoriesResponse
    {

        [JsonProperty("categories")]
        public List<SerializedCategory> Categories { get; set; }
    }
}
