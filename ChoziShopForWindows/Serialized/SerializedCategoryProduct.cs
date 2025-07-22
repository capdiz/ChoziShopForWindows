using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.Serialized
{
    public class SerializedCategoryProduct
    {
        [JsonProperty("category_section_id")]
        public long CategorySectionId { get; set; }
        [JsonProperty("product_name")]
        public string ProductName { get; set; }
        [JsonProperty("remarks")]
        public string Remarks { get; set; }
        [JsonProperty("tag")]
        public string Tag { get; set; }
        [JsonProperty("value_metric")]
        public string ValueMetric { get; set; }
        [JsonProperty("unit_cost_cents")]
        public decimal UnitCostCents { get; set; }
        [JsonProperty("units")]
        public decimal Units { get; set; }
        [JsonProperty("measurement")]
        public string Measurement { get; set; }
        [JsonProperty("currency")]
        public string Currency { get; set; }


    }
}
