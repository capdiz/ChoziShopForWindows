using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


namespace ChoziShopForWindows.Serialized
{
    public class SerializedCategory
    {
        [JsonProperty("id")]
        public long OnlineCategoryId { get; set; }
        [JsonProperty("category_name")]
        public string CategoryName { get; set; }

        [JsonProperty("icon_url")]
        public string IconUrl { get; set; }

        [JsonProperty("updated_at")]
        [JsonConverter( typeof(IsoDateTimeConverter))]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("default_product_count")]
        public int DefaultProductsCount;

    }
}