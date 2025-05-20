using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.Data
{
    public class AirtelPaymentCollectionRequest
    {
        private const string REFERENCE = "ChoziShop";
        private const string COUNTRY = "UG";
        private const string CURRENCY = "UGX";

        [JsonProperty("reference")]
        public string Reference {
            get { return REFERENCE; }
        }
        [JsonProperty("country")]
        public string Country {
            get { return COUNTRY; }
        }
        [JsonProperty("currency")]
        public string Currency { get { return CURRENCY; } }
        [JsonProperty("msisdn")]
        public required string Msisdn { get; set; }
        [JsonProperty("amount")]
        public decimal Amount { get; set; }
        [JsonProperty("collection_request")]
        public AirtelPaymentCollectionRequestResponse? CollectionRequest { get; set; }
        public bool ShouldSerializeCollectionRequest() => false;

    }
}
