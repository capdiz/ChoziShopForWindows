using ChoziShopForWindows.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.Services
{
    public interface IAuthTokenProvider
    {
        string? GetPaymentAuthToken();
        void SetPaymentAuthToken(string? paymentAuthToken);
        string? GetCurrentUserAuthToken();
        void SetCurrentUserAuthToken(string? currentUserAuthToken);
        MerchantAccount GetCurrentMerchantAccount();
        void SetCurrentMerchantAccount(MerchantAccount? currentUserAccount);

    }
}
