using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.Serialized
{
    public class SerializedResponse
    {
        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;
        [JsonProperty("deleted")]
        public int Deleted { get; set; }
        [JsonProperty("deleted_at")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime? DeletedAt { get; set; }
        [JsonProperty("errors")]
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
