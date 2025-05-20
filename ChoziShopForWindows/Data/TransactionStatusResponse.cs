using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.Data
{
    public class TransactionStatusResponse
    {
        [JsonProperty("collection_response")]
        public CollectionRequest? AirtelCollectionResponse { get; set; }

        public class CollectionRequest
        {
            [JsonProperty("status")]
            public string? Status { get; set; }
            [JsonProperty("airtel_money_id")]
            public string? AirtelTransactionId { get; set; }
            [JsonProperty("updated_at")]
            [JsonConverter(typeof(IsoDateTimeConverter))]
            public DateTime UpdatedAt { get; set; }
        }
    }
}
