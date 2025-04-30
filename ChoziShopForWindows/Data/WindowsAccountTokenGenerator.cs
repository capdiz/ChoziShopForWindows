using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.Data
{
    public class WindowsAccountTokenGenerator
    {
        public string generateWindowsAccountToken()
        {
            string machineName = WebUtility.UrlEncode(Environment.MachineName);

            string randomText = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');

            return $"{machineName}-{randomText}-{DateTime.Now.Ticks}";
        }

        public string generateLoginToken()
        {
            string machineName = WebUtility.UrlEncode(Environment.MachineName);
            string randomText = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');
            return $"{machineName}-{randomText}-{DateTime.Now.Ticks}";
        }
    }
}
