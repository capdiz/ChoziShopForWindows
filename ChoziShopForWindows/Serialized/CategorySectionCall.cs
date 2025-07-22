using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChoziShopForWindows.Serialized
{
    public class CategorySectionCall
    {
        [JsonProperty("category_ids")]
        public List<long> CategoryIds { get; set; }
    }
}
