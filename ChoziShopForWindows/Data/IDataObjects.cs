using ChoziShop.Data.Models;
using ChoziShop.Data.Repository;
using ChoziShopForWindows.Enums;
using ChoziShopForWindows.models;
using ChoziShopForWindows.Serialized;
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
        IRepository<Category> Categories { get; }   
        IRepository<CategoryProduct> CategoryProducts { get; }
        IRepository<CategorySection> CategorySections { get; }

        IRepository<MerchantSession> MerchantSessions { get; }

        IRepository<Order> ShopOrders { get; }
        IRepository<PaymentAuth> PaymentAuths { get; }
        IRepository<AirtelPayCollection> AirtelCollections { get; }
        IRepository<Keeper> Keepers { get; }
        IRepository<DefaultCategorySectionProduct> DefaultCategorySectionProducts { get; }
        IRepository<InventoryTracker> InventoryTrackers { get; }


        Task SaveAsync();
        void Rollback();

        Task SaveMerchantAsync(Merchant merchant);
        Task<Merchant> SaveAndReturnMerchantAsync(Merchant merchant);

        Task<Store> SaveAndReturnStoreAsync(Store store);

        Task<CategorySection> AddCategorySectionAsync(CategorySection categorySection);

        Task<Keeper> FindKeeperByOnlineId(long onlineKeeperId);
        Task<Keeper> FindKeeperByEmailAsync(string email);

        Task<Order> GetOrderByOnlineId(long onlineOrderId);

        Task<List<Keeper>> GetAllKeepersAsync();
        Task<Merchant> GetMerchantAsync();

        Task AddCategoryProductAsync(CategoryProduct categoryProduct);

        Task CreateMerchantSessionAsync(MerchantSession merchantSession);

        Task UpdateMerchantSession(MerchantSession merchantSession);
        Task<IEnumerable<CategoryProduct>> SearchCategoryProductsAsync(string searchQuery);
        Task<PaymentAuth> GetPaymentAuth();
        Task<PaymentAuth> CreatePaymentAuth(PaymentAuth paymentAuth);
        Task<AirtelPayCollection> CreateAirtelPayCollection(AirtelPayCollection airtelPayCollection);
        Task<bool> UpdateAirtelPayCollection(AirtelPayCollection airtelPayCollection);

        Task<Store?> GetDefaultStoreAsync();

        Task<List<Order>> GetTodaysOrdersAsync();
        Task<List<Order>> GetCurrentWeekEntitiesAsync();
        Task<List<Order>> GetCurrentMonthEntitiesAsync();

        Task<List<TopSellingCategoryProduct>> GetTopSellingCategoryProductsAsync(int count = 10);
        Task<List<TopSellingCategory>> GetTopSellingCategoriesAsync(int count = 10);
        Task<List<TopSellingCategory>> GetTopSellingCategoriesByScopeAsync(GroupByScope scope,
            DateTime? startDate = null, DateTime? endDate = null, int limit = 10);

        Task<List<TopSellingCategoryProduct>> GetTopSellingCategoryProductsByScope(GroupByScope scope,
            DateTime? startDate = null, DateTime? endDate = null, int limit = 10);

        Task TrackInventoryChange(InventoryTracker inventoryTracker);

        Task<List<CategoryProduct>> GetCategoryProductsAsync();

        List<StockItem> GroupStockItemsByStatusAsync(List<CategoryProduct> categoryProducts, ProductInventoryStatus status);
        Task<List<InventoryTracker>>  FindProductInventoryLogHistory(int categoryProductId);

        Task<CategoryProduct> GetCategoryProductByIdAsync(int id);
        Task<InventoryTracker> GetLastInventoryTrackerForProduct(int categoryProductId);
        

        Task<Store> GetDefaultUserAccountStore();

        Task<List<CategorySection>> GetStoreCategorySectionsAsync(long inventoryId);
        Task<List<CategorySection>> GetAllCategorySectionsAsync();
        Task<List<CategoryProduct>> GetCategoryProductsByCategorySectionId(int categorySectionId);
        Task<List<Order>> GetStoreOrdersAsync(long onlineStoreId);

        Task<List<Order>> GetCurrentDayPendingOrdersAsync();
        Task<List<Category>> GetCachedDefaultCategories(IEnumerable<string> existingCategorySectionNames);
        Task<List<DefaultCategorySectionProduct>> GetCategorySectionDefaultProductsAsync(IEnumerable<long> existingCategorySectionIds);
        Task<List<DefaultCategorySectionProduct>> GetCategorySectionDefaultProductsByCategorySectionIdAsync(long categorySectionId);
        Task<long> GetOnlineCategoryIdByName(string categoryName);
        Task<bool> SoftDeleteCategoryProductAsync(int categoryProductId);
        Task<long> GetCategorySectionOnlineId(int categorySectionId);

        Task<List<AirtelPayCollection>> GetAirtelPayCollectionsAsync();
    }
}
