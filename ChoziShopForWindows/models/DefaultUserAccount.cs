using ChoziShop.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.models
{
    public class DefaultUserAccount
    {
        private KeeperAccount _keeperAccount;
        private MerchantAccount _merchantAccount;
        private Store _currentUserStore;
        public DefaultUserAccount(MerchantAccount merchant)
        {
            OnlineUserAccountId = merchant.OnlineMerchantId;
            FullName = merchant.FullName;
            Email = merchant.Email;
            PhoneNumber = merchant.PhoneNumber;
            AuthToken = merchant.AuthToken;
            BareJid = merchant.BareJid;
            FullJid = merchant.FullJid;
            AccountType = 1; // Merchant
            DefaultAccountType = "merchant";
        }

        public DefaultUserAccount(Keeper keeper)
        {
            OnlineUserAccountId = keeper.OnlineKeeperId;
            FullName = keeper.FullName;
            Email = keeper.Email;
            PhoneNumber = keeper.PhoneNumber;
            AuthToken = keeper.AuthToken;
            BareJid = keeper.BareJid;
            FullJid = keeper.FullJid;
            AccountType = 0; // Keeper
            DefaultAccountType = "keeper";
        }

        public int AccountType { get; set; } // 1 for Merchant, 0 for keeper
        public long OnlineUserAccountId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;
        public string AuthToken { get; set; } = string.Empty;

        public string BareJid { get; set; } = string.Empty;
        public string FullJid { get; set; } = string.Empty;
        public string DefaultAccountType { get; set; }

        public void SetMerchantAccount(MerchantAccount merchant)
        {
            _merchantAccount = new MerchantAccount
            {
                OnlineMerchantId = merchant.OnlineMerchantId,
                FullName = merchant.FullName,
                Email = merchant.Email,
                PhoneNumber = merchant.PhoneNumber,
                AuthToken = merchant.AuthToken,
                BareJid = merchant.BareJid,
                FullJid = merchant.FullJid
            };
        }

        public MerchantAccount GetMerchantAccount()
        {
            if (_merchantAccount == null)
            {
                throw new InvalidOperationException("Merchant account is not set.");
            }
            return _merchantAccount;
        }

        public void SetKeeperAccount(Keeper keeper)
        {
            _keeperAccount = new KeeperAccount
            {
                OnlineKeeperId = keeper.OnlineKeeperId,
                FullName = keeper.FullName,
                Email = keeper.Email,
                PhoneNumber = keeper.PhoneNumber,
                AuthToken = keeper.AuthToken,
                BareJid = keeper.BareJid,
                FullJid = keeper.FullJid
            };
        }

        public KeeperAccount GetKeeperAccount()
        {
            if (_keeperAccount == null)
            {
                throw new InvalidOperationException("Keeper account is not set.");
            }
            return _keeperAccount;
        }

        public void SetCurrentUserStore(Store store)
        {
            _currentUserStore = store;
        }

        public Store GetCurrentUserStore()
        {
            if (_currentUserStore == null)
            {
                throw new InvalidOperationException("Current user store is not set.");
            }
            return _currentUserStore;
        }
    }
}
