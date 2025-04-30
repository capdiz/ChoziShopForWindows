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
        IRepository<Store> Stores { get; }

        IRepository<CategorySection> CategorySections { get; }

        IRepository<MerchantSession> MerchantSessions { get; }

        Task SaveAsync();
        void Rollback();

        Task SaveMerchantAsync(Merchant merchant);  
        Task<Merchant> SaveAndReturnMerchantAsync(Merchant merchant);
        
        Task<Store> SaveAndReturnStoreAsync(Store store);

        Task<CategorySection> AddCategorySectionAsync(CategorySection categorySection);
         
        Task AddCategoryProductAsync(CategoryProduct categoryProduct);

        Task CreateMerchantSessionAsync(MerchantSession merchantSession);

        Task UpdateMerchantSession(MerchantSession merchantSession);    
    }
}
