using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.Enums
{
    public enum TransactionStatus
    {
        TIP, // TransactionInProgress
        TF, // TransactionFailed
        TS, // TransactionSuccessful
        INVALID_PIN,
        INSUFFICIENT_FUNDS,
        TRANSACTION_LIMIT_HIT,
        TTO,// TransactionTimeOut
        FAILED
    }
}
