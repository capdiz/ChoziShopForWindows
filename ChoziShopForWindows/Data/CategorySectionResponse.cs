using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.Data
{
    public class CategorySectionResponse
    {
        [JsonProperty("id")]
        public long Id { get; set; }
        [JsonProperty("inventory_id")]
        public long InventoryId { get; set; }
        [JsonProperty("category_name")]
        public required string CategoryName { get; set; }
        [JsonProperty("created_at")]
        [JsonConverter(typeof(IsoDateTimeConverter))]   
        public DateTime CreatedAt { get; set; }
        [JsonProperty("updated_at")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime UpdatedAt { get; set; }
        [JsonProperty("category_products")]
        public List<CategoryProductResponse> CategoryProducts { get; set; } = new();


    }
}
