using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.Serialized
{
    public class CategoryProductResponse
    {
        [JsonProperty("id")]
        public long Id { get; set; }
        [JsonProperty("category_section_id")]
        public long CategorySectionId { get; set; }
        [JsonProperty("product_name")]
        public required string ProductName { get; set; }
        [JsonProperty("remarks")]
        public string Remarks { get; set; }
        [JsonProperty("tag")]
        public string Tag { get; set; }
        [JsonProperty("measurement")]
        public string Measurement { get; set; } 
        [JsonProperty("currency")]
        public string Currency { get; set; } 
        [JsonProperty("value_metric")]
        public string ValueMetric { get; set; }
        [JsonProperty("unit_cost_cents")]
        public decimal UnitCostCents { get; set; }
        [JsonProperty("units")]
        public decimal Units { get; set; }
        
        [JsonProperty("created_at")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime CreatedAt { get; set; } 
        [JsonProperty("updated_at")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime UpdatedAt { get; set; } 
        [JsonProperty("barcode_url")]
        public string BarcodeUrl { get; set; }
    }
}
