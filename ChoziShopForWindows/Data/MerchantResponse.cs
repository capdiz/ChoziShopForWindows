using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.Data
{
    public class MerchantResponse
    {
        [JsonProperty("id")]
        public long Id { get; set; }
        [JsonProperty("merchant_id")]
        public long OnlineMerchantId { get; set; }
        [JsonProperty("full_name")]
        public string FullName { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("phone_number")]
        public string PhoneNumber { get; set; }
        [JsonProperty("country_of_operations")]
        public string CountryOfOperations { get; set; }
        [JsonProperty("auth_password")]
        public string AuthPassword { get; set; }
        [JsonProperty("created_at")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime CreatedAt { get; set; }
        [JsonProperty("updated_at")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime UpdatedAt { get; set; }
        [JsonProperty("merchant_auth_token")]
        public string AuthToken {  get; set; }
        [JsonProperty("bare_jid")]
        public string BareJid { get; set; }
        [JsonProperty("full_jid")]
        public string FullJid { get; set; }



    }
}
