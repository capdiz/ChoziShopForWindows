using ChoziShop.Data.Models;
using ChoziShopForWindows.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.MerchantsApi
{
    public class BaseApi : HttpService
    {

        public BaseApi(string authToken) : base(authToken) { }

        public async Task<List<StoreResponse>> GetWindowsAccountStores(long merchantId)
        {
            Debug.WriteLine("Fetching store details for merchant no: " + merchantId);
            return await GetListAsync<StoreResponse>($"{Store.WindowsAccountStoreUrl}?merchant_id={merchantId}");

        }



    }

}
