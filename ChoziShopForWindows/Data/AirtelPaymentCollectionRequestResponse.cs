using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.Data
{
     public class AirtelPaymentCollectionRequestResponse
    {
        [JsonProperty("airtel_pay_collection_request_id")]
        public long AirtelPayCollectionRequestId { get; set; }
        [JsonProperty("status")]
        public required string Status { get; set; }
        [JsonProperty("transaction_id")]
        public required string TransactionId { get; set; }
        [JsonProperty("created_at")]
        public required string CreatedAt { get; set; }
    }
}
