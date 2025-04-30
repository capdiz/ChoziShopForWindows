using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.models
{
    public class MerchantAccount
    {
        public long OnlineMerchantId { get; set; }
        public string FullName { get; set; }=string.Empty;
        public string Email { get; set; }=string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string AuthToken { get; set; } = string.Empty;
        public string BareJid { get; set; } = string.Empty;
        public string FullJid { get; set; } = string.Empty;
    }
}
