using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.Data
{
    public class StoreResponse
    {
        private Inventory inventory = new Inventory();

        [JsonProperty("id")]
        public long Id { get; set; }
        [JsonProperty("merchant_id")]
        public long MerchantId { get; set; }    
        [JsonProperty("store_name")]
        public required string StoreName { get; set; }
        [JsonProperty("country_of_operations")]
        public required string CountryOfOperations { get; set; }
        [JsonProperty("latitude")]
        public required string Latitude { get; set; }
        [JsonProperty("longitude")]
        public required string Longitude { get; set; }
        [JsonProperty("created_at")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime CreatedAt { get; set; }
        [JsonProperty("updated_at")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime UpdatedAt { get; set; }
        [JsonProperty("opening_time")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime OpeningTime { get; set; }
        [JsonProperty("closing_time")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime ClosingTime { get; set; }
        [JsonProperty("shop_code")]
        public required string ShopCode { get; set; }
        [JsonProperty("location_name")]
        public required string LocationName { get; set; }
        [JsonProperty("base_location")]
        public required string BaseLocation { get; set; }
        [JsonProperty("directions")]
        public required string Directions { get; set; }
        [JsonProperty("inventory")]
        public required Inventory StoreInventory { get; set; }
       

        public class Inventory
        {
            [JsonProperty("id")]
            public long Id { get; set; }

        }

    }
}
