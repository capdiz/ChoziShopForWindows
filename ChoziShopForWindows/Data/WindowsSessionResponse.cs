using Microsoft.EntityFrameworkCore.Query.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.Data
{
    public class WindowsSessionResponse
    {
        [JsonProperty("id")]
        public long Id { get; set; }
        [JsonProperty("auth_token")]
        public required string AuthToken { get; set; }
        [JsonProperty("device_token")]
        public required string DeviceToken { get; set; }
        [JsonProperty("status")]
        public required string Status { get; set; }
        [JsonProperty("expires_at")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime ExpiresAt { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }

        public int ResponseCode { get; set; }

        public WindowsSessionResponse() { }
    }
}
