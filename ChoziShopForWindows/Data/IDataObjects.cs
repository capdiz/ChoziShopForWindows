using ChoziShop.Data.Models;
using ChoziShop.Data.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.Data
{
    public interface IDataObjects
    {
        IRepository<UnSyncedObject> UnSyncedObjects { get; }
        IRepository<Merchant> Merchants { get; }
        Task SaveAsync();
        void Rollback();
    }
}
