using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.Data
{
    public class PaymentAuthRequest
    {
        [JsonProperty("email")]
        public  string Email { get; set; }
        [JsonProperty("access_id")]
        public string AccessId { get; set; }
        [JsonProperty("auth_token")]
        public string AuthToken { get; set; }
        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }
        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; }

       

    }
}
