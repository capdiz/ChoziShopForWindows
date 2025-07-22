using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.Serialized
{
    public class SerializedKeeper
    {
        [JsonProperty("id")]
        public long Id { get; set; }
        [JsonProperty("keeper_id")]
        public long KeeperId { get; set; }
        [JsonProperty("store_id")]
        public long StoreId { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("phone_number")]
        public string PhoneNumber { get; set; }
        [JsonProperty("created_at")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime CreatedAt { get; set; }
        [JsonProperty("updated_at")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime UpdatedAt
        {
            get; set;
        }
        [JsonProperty("bare_jid")]
        public string BareJid { get; set; }
        [JsonProperty("full_jid")]
        public string FullJid { get; set; }
        [JsonProperty("access_key_id")]
        public string AccessKeyId { get; set; }
        [JsonProperty("auth_token")]
        public string AuthToken { get; set; }
        [JsonProperty("invitation_confirmed_at")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime InvitationConfirmedAt { get; set; }

    }
}
