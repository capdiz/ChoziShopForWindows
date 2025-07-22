using ChoziShopForWindows.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.Serialized
{
    public class SerializedCategorySectionResponse
    {
        [JsonProperty("category_sections")]
        public List<CategorySectionResponse> CategorySections { get; set; } = new();
    }
}
