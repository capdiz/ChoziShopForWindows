using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.Serialized
{
    public class SerializedCategorySection
    {
        [JsonProperty("id")]
        public long Id { get; set; }
        [JsonProperty("inventory_id")]
        public long InventoryId { get; set; }
        [JsonProperty("category_id")]
        public long CategoryId { get; set; }
        [JsonProperty("category_name")]
        public string CategoryName { get; set; }
        
        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }
        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; }
        [JsonProperty("product_items_count")]
        public int ProductItemsCount { get; set; }
        [JsonProperty("barcode_pdf_url")]
        public string BarcodePdfUrl { get; set; }


    }
}
