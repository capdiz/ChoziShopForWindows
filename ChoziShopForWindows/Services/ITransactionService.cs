using ChoziShop.Data.Models;
using ChoziShopForWindows.Data;
using ChoziShopForWindows.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.Services
{
    public interface ITransactionService
    {
        Task<TransactionStatus> CheckTransactionStatusAsync(AirtelPayCollection airtelPayCollection, IDataObjects dataObjects);
        void SetAuthTokenProvider(IAuthTokenProvider authTokenProvider);
    }
}
