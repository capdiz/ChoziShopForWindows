using ChoziShop.Data.Models;
using ChoziShopForWindows.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.Services
{
    public class AuthTokenProvider : IAuthTokenProvider
    {
        private MerchantAccount _merchantAccount;
        private string? _paymentAuthToken;
        private string? _currentUserAuthToken;

        public void SetPaymentAuthToken(string? authToken)
        {
            _paymentAuthToken = authToken;
        }



        public string? GetPaymentAuthToken()
        {
            return _paymentAuthToken;
        }

        public void SetCurrentUserAuthToken(string? currentUserAuthToken)
        {
            _currentUserAuthToken = currentUserAuthToken;
        }

        public string? GetCurrentUserAuthToken()
        {
            return _currentUserAuthToken;
        }

        public void SetCurrentMerchantAccount(MerchantAccount? merchantAccount)
        {
            _merchantAccount = merchantAccount;
        }

        public MerchantAccount GetCurrentMerchantAccount()
        {
            return _merchantAccount;
        }

         
    }
}
