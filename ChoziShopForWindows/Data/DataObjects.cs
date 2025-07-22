using ChoziShop.Data.Enums;
using ChoziShop.Data.Models;
using ChoziShop.Data.Repository;
using ChoziShop.Data.Repository.Extensions;
using ChoziShopForWindows.Enums;
using ChoziShopForWindows.models;
using ChoziShopForWindows.Serialized;
using ChoziShopForWindows.Validations;
using HandyControl.Data;
using LiveCharts.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;


namespace ChoziShopForWindows.Data
{
    public class DataObjects : IDataObjects, IDisposable
    {
        private readonly DatabaseContext _context;


        public DataObjects(DatabaseContext context)
        {
            _context = context;
            UnSyncedObjects = new Repository<UnSyncedObject>(_context);
            Merchants = new MerchantRepository(_context);
            Stores = new Repository<Store>(_context);
            Categories=new Repository<Category>(_context);  
            CategorySections = new Repository<CategorySection>(_context);
            CategoryProducts = new Repository<CategoryProduct>(_context);
            MerchantSessions = new Repository<MerchantSession>(_context);
            ShopOrders = new Repository<Order>(_context);
            PaymentAuths = new Repository<PaymentAuth>(_context);
            AirtelCollections = new Repository<AirtelPayCollection>(_context);
            TopSellingCategoryProducts = new Repository<TopSellingCategoryProduct>(_context);
            InventoryTrackers = new Repository<InventoryTracker>(_context);
            Keepers=new Repository<Keeper>(_context);
            DefaultCategorySectionProducts = new Repository<DefaultCategorySectionProduct>(_context);
        }

        public IRepository<UnSyncedObject> UnSyncedObjects { get; }

        public IRepository<Merchant> Merchants { get; }
        public IRepository<Store> Stores { get; }
        public IRepository<Category> Categories { get; }
        public IRepository<CategorySection> CategorySections { get; }
        public IRepository<CategoryProduct> CategoryProducts { get; }
        public IRepository<MerchantSession> MerchantSessions { get; }
        public IRepository<Order> ShopOrders { get; }

        public IRepository<PaymentAuth> PaymentAuths { get; }
        public IRepository<AirtelPayCollection> AirtelCollections { get; }
        public IRepository<TopSellingCategoryProduct> TopSellingCategoryProducts { get; }
        public IRepository<InventoryTracker> InventoryTrackers { get; }
        public IRepository<Keeper> Keepers { get; }
        public IRepository<DefaultCategorySectionProduct> DefaultCategorySectionProducts { get; }   
        



        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task SaveMerchantAsync(Merchant merchant)
        {
            try
            {
                await _context.AddAsync(merchant);
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving merchant: " + ex.Message);
            }
        }

        public async Task<Merchant> SaveAndReturnMerchantAsync(Merchant merchant)
        {
            try
            {
                merchant = await Merchants.SaveAndReturnEntityAsync(merchant);
                return merchant;
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving and returning merchant: " + ex.Message);
            }
        }

        public async Task<List<Keeper>> GetAllKeepersAsync()
        {
            try
            {
                return await Keepers.GetQueryable()
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting all keepers: " + ex.Message);
            }
        }

        public async Task<Merchant> GetMerchantAsync()
        {
            try
            {
                return await Merchants.GetQueryable()
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting merchant: " + ex.Message);
            }
        }

        public async Task<Keeper> FindKeeperByOnlineId(long onlineKeeperId)
        {
            return await Keepers.GetQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(kp => kp.OnlineKeeperId == onlineKeeperId);
        }

        public async Task<Keeper> FindKeeperByEmailAsync(string email)
        {
            return await Keepers.GetQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(kp => kp.Email == email);
        }

        public async Task<Order> GetOrderByOnlineId(long onlineOrderId)
        {
            try
            {
                return await ShopOrders.GetQueryable()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.OnlineOrderId == onlineOrderId);
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting order by online ID: " + ex.Message);
            }
        }

        public async Task<CategoryProduct> GetCategoryProductByIdAsync(int id)
        {
            try
            {
                return await CategoryProducts.GetQueryable()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(cp => cp.Id == id);
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting category product by ID: " + ex.Message);
            }
        }

        public async Task<InventoryTracker> GetLastInventoryTrackerForProduct(int productId)
        {
            try
            {
                return await InventoryTrackers.GetQueryable()
                    .AsNoTracking()
                    .Where(tracker => tracker.CategoryProductId == productId)
                    .OrderByDescending(tracker => tracker.CreatedAt)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting last inventory tracker for product: " + ex.Message);
            }
        }

        

        public async Task<Store> SaveAndReturnStoreAsync(Store store)
        {
            try
            {
                store = await Stores.SaveAndReturnEntityAsync(store);
                await _context.SaveChangesAsync();
                return store;
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving and returning store: " + ex.Message);
            }
        }

        public async Task<CategorySection> AddCategorySectionAsync(CategorySection categorySection)
        {
            try
            {
                categorySection = await CategorySections.SaveAndReturnEntityAsync(categorySection);
                await _context.SaveChangesAsync();
                return categorySection;
            }           
            catch (Exception ex)
            {
                throw new Exception("Error adding category section: " + ex.Message);
            }
        }

        public async Task AddCategoryProductAsync(CategoryProduct categoryProduct)
        {
            try
            {
                await _context.AddAsync(categoryProduct);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding category product: " + ex.Message);
            }
        }

        public async Task CreateMerchantSessionAsync(MerchantSession merchantSession)
        {
            try
            {
                await _context.AddAsync(merchantSession);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating merchant session: " + ex.Message);
            }
        }

        public async Task UpdateMerchantSession(MerchantSession merchantSession)
        {
            try
            {
                await MerchantSessions.UpdateAsync(merchantSession);
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating merchant session: " + ex.Message);
            }
        }

        public async Task<List<Order>> GetOrdersByDateAndStatus(DateTime createdAt, CustomerOrderStatus customerOrderStatus, ChoziShop.Data.Enums.SortDirection sortDirection)
        {
            try
            {
                return await ShopOrders.SortOrderByDateAndOrderStatus(createdAt, customerOrderStatus, sortDirection);
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting orders by date and status: " + ex.Message);
            }
        }

        public async Task<List<AirtelPayCollection>> GetAirtelPayCollectionsAsync()
        {
            try
            {
               return await AirtelCollections.GetQueryable()
                    .AsNoTracking()
                    .OrderByDescending(c => c.PaymentInitiatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting Airtel Pay collections: " + ex.Message);
            }
        }

        public async Task<PaymentAuth> GetPaymentAuth()
        {
            try
            {
                return await PaymentAuths.GetQueryable()
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting payment auth: " + ex.Message);
            }
        }

        public async Task<PaymentAuth> CreatePaymentAuth(PaymentAuth paymentAuth)
        {
            try
            {
                await PaymentAuths.AddAsync(paymentAuth);
                await _context.SaveChangesAsync();
                return paymentAuth;
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating payment auth: " + ex.Message);
            }
        }

        public async Task<AirtelPayCollection> CreateAirtelPayCollection(AirtelPayCollection airtelPayCollection)
        {
            try
            {
                await AirtelCollections.AddAsync(airtelPayCollection);
                await _context.SaveChangesAsync();
                return airtelPayCollection;
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating Airtel Pay collection: " + ex.Message);
            }
        }

        public async Task<IEnumerable<CategoryProduct>> SearchCategoryProductsAsync(string searchTerm)
        {
            var ftsQuery = $"\'{searchTerm}*\'";
            var query = @$"SELECT c.id, c.category_section_id, c.online_category_product_id, c.product_name, c.barcode_url, c.created_at, "+
                "c.currency, c.updated_at, c.item_code, c.units, c.unit_cost, c.measurement, c.remarks, c.tag, c.value_metric, c.online_category_section_id, c.isDeleted " +
                $"FROM CategoryProducts c INNER JOIN CategoryProductFts ON c.id = CategoryProductFts.rowid WHERE CategoryProductFts MATCH {ftsQuery} ORDER BY rank";

            return await _context.CategoryProducts
                .FromSqlRaw(query)
                .AsNoTracking()
                .ToListAsync();
        }

        public void Rollback()=>_context.ChangeTracker.Clear();

        public async Task<bool> UpdateAirtelPayCollection(AirtelPayCollection airtelPayCollection)
        {
            try
            {
                _context.Entry(airtelPayCollection).State = EntityState.Modified;
                var changes = await _context.SaveChangesAsync();
                return changes > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating Airtel Pay collection: " + ex.Message);
            }
        }

        public async Task<Store?> GetDefaultStoreAsync()
        {
            var store = await Stores.GetQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync();
            return store;
        }

        public async Task<List<Order>> GetTodaysOrdersAsync()
        {
            var _todaysOrders = await ShopOrders.GetTodaysOrdersAsync();
            return _todaysOrders;
        }

        public async Task<List<Order>> GetCurrentWeekEntitiesAsync()
        {
            var _currentWeekEntities = await ShopOrders.GetCurrentWeekEntitiesAsync();
            return _currentWeekEntities;
        }

        public async Task<List<Order>> GetCurrentMonthEntitiesAsync()
        {
            var _currentMonthEntities = await ShopOrders.GetCurrentMonthEntitiesAsync();
            return _currentMonthEntities;
        }

        public async Task<List<CategoryProduct>> GetCategoryProductsByCategorySectionId(int categorySectionId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<CategorySection>> GetAllCategorySectionsAsync()
        {
            return await _context.CategorySections.ToListAsync();
        }

       

        public async Task<List<TopSellingCategoryProduct>> GetTopSellingCategoryProductsAsync(int limit = 10)
        {
            var orders = await _context.CustomerOrders                
                .ToListAsync();

            var productSales = orders.SelectMany(o => o.OrderProductItems
                .Select(item => new
                {
                    OnlineItemId = item.OnlineCategoryProductId,
                    ItemName = item.ProductName,
                    SaleDate = o.CreatedAt

                }))
                .GroupBy(cp => cp.OnlineItemId)
                .Select(g => new
                {
                    OnlineItemId = g.Key,
                    ProductName = g.First().ItemName,
                    SalesCount = g.Count(),
                    LastSaleDate = g.Max(cp => cp.SaleDate),
                    DaysSinceLastSale = (DateTime.Now - g.Max(cp => cp.SaleDate)).TotalDays
                }).Select(x => new TopSellingCategoryProduct
                {
                    OnlineCategoryProductId = x.OnlineItemId,
                    ProductName = x.ProductName,
                    TotalSalesCount = x.SalesCount,
                    LastSaleDate = x.LastSaleDate,
                    RecencyWeightedScore = (x.SalesCount * 100.0) / (1 + x.DaysSinceLastSale)
                })
                .OrderByDescending(x => x.RecencyWeightedScore)
                .ThenByDescending(x => x.LastSaleDate)
                .ThenByDescending(x => x.TotalSalesCount)
                .Take(limit)
                .ToList();
          
            return productSales;
        }

        public async Task<List<InventoryTracker>> FindProductInventoryLogHistory(int categoryProductId)
        {
            var _invenroyTrackers = await _context.InventoryTrackers.Where(tracker => tracker.CategoryProductId == categoryProductId).ToListAsync();
            return _invenroyTrackers;
        }

        public async Task<List<CategorySection>> GetStoreCategorySectionsAsync(long inventoryId)
        {
            var categorySections = await _context.CategorySections.Where(cs => cs.InventoryId == inventoryId).ToListAsync();
            return categorySections;
        }

        public async Task<List<Order>> GetStoreOrdersAsync(long onlineStoreId)
        {
            var orders = await _context.CustomerOrders.Where(co => co.StoreId == onlineStoreId).ToListAsync();
            return orders;
        }

        public async Task<List<Order>> GetCurrentDayPendingOrdersAsync()
        {
            var today = DateTime.UtcNow.Date;
            var pendingOrders = await _context.CustomerOrders
                .Where(o => o.CreatedAt.Date == today && o.OrderStatus == (int) CustomerOrderStatus.Held)
                .ToListAsync();
            return pendingOrders;
        }

        public async Task<List<TopSellingCategoryProduct>> GetTopSellingCategoryProductsByScope(GroupByScope scope, DateTime? startDate = null, DateTime? endDate = null, int limit = 10)
        {
            var defaultStartDate = DateTime.UtcNow;
            var defaultEndDate = DateTime.UtcNow;
            Debug.WriteLine(" Scope: " + scope + ", StartDate: " + startDate + ", EndDate: " + endDate);

            var orders = await _context.CustomerOrders
                .Where(o => o.CreatedAt >= (startDate ?? defaultStartDate) &&
                o.CreatedAt <= (endDate ?? defaultEndDate))
                .Select(o => new { o.CreatedAt, o.OrderProductItems })
                .ToListAsync();
            var productSales = orders.SelectMany(o => o.OrderProductItems
            .Select(item => item.OnlineCategoryProductId)
            .Distinct()
            .Select(productId => new
            {
                ProductId = productId,
                ItemName = o.OrderProductItems.FirstOrDefault(item => item.OnlineCategoryProductId == productId)?.ProductName ?? "Unknown Product",
                UnitPrice = o.OrderProductItems.FirstOrDefault(item => item.OnlineCategoryProductId == productId)?.UnitCost ?? 0,
                UnitsSold = o.OrderProductItems.Where(cp => cp.OnlineCategoryProductId == productId).Count(),
                CreatedAt = o.CreatedAt
            })).ToList();

            switch (scope)
            {
                case GroupByScope.Day:
                    var todaysProductSales = productSales.GroupBy(productSale => new
                    {
                        productSale.ProductId,
                        productSale.ItemName,
                        productSale.UnitPrice,
                        productSale.UnitsSold,
                        productSale.CreatedAt
                    }).Select(g => new
                    {
                        ProductId = g.Key.ProductId,
                        ItemName = g.Key.ItemName,
                        TotalSalesAmount = g.Sum(cp => cp.UnitPrice * cp.UnitsSold),
                        SalesCount = g.Count(),
                        LastSaleDate = g.Max(cp => cp.CreatedAt),
                        DaysSinceLastSale = (DateTime.Now - g.Max(cp => cp.CreatedAt)).TotalDays
                    }).ToList();
                    var todaysUniqueProductSales = todaysProductSales.GroupBy(x => x.ProductId)
                        .Select(g => new
                        {
                            ProductId = g.Key,
                            ItemName = g.First().ItemName,
                            TotalSalesAmount = g.Sum(cp => cp.TotalSalesAmount),
                            SalesCount = g.Sum(cp => cp.SalesCount),
                            LastSaleDate = g.Max(cp => cp.LastSaleDate),
                            DaysSinceLastSale = (DateTime.Now - g.Max(cp => cp.LastSaleDate)).TotalDays
                        })
                    .Select(x => new TopSellingCategoryProduct
                    {
                        OnlineCategoryProductId = x.ProductId,
                        ProductName = x.ItemName,
                        TotalSalesAmount = x.TotalSalesAmount,
                        TotalSalesCount = x.SalesCount,
                        LastSaleDate = x.LastSaleDate,
                        RecencyWeightedScore = (x.SalesCount * 100.0) / (1 + x.DaysSinceLastSale)

                    })
                    .OrderByDescending(x => x.RecencyWeightedScore)
                    .ThenByDescending(x => x.LastSaleDate)
                    .ThenByDescending(x => x.TotalSalesCount)
                    .Take(limit)
                    .ToList();
                    return todaysUniqueProductSales;
                case GroupByScope.Week:
                    var weeksProductSales = productSales.GroupBy(productSale => new
                    {
                        productSale.ProductId,
                        productSale.ItemName,
                        productSale.UnitPrice,
                        productSale.UnitsSold,
                        SaleDate = productSale.CreatedAt,
                        Week = (productSale.CreatedAt.Day - 1) / 7 + 1 // Calculate week number in the month
                    }).Select(g => new
                    {
                        ProductId = g.Key.ProductId,
                        ItemName = g.Key.ItemName,
                        TotalSalesAmount = g.Sum(cp => cp.UnitPrice * cp.UnitsSold),
                        SalesCount = g.Count(),
                        LastSaleDate = g.Max(cp => cp.CreatedAt),
                        DaysSinceLastSale = (DateTime.Now - g.Max(cp => cp.CreatedAt)).TotalDays
                    }).ToList();
                    var uniqueWeeksProductSales = weeksProductSales.GroupBy(x => x.ProductId)
                        .Select(g => new
                        {
                            ProductId = g.Key,
                            ItemName = g.First().ItemName,
                            TotalSalesAmount = g.Sum(cp => cp.TotalSalesAmount),
                            SalesCount = g.Sum(cp => cp.SalesCount),
                            LastSaleDate = g.Max(cp => cp.LastSaleDate),
                            DaysSinceLastSale = (DateTime.Now - g.Max(cp => cp.LastSaleDate)).TotalDays
                        })
                        .Select(x => new TopSellingCategoryProduct
                        {
                            OnlineCategoryProductId = x.ProductId,
                            ProductName = x.ItemName,
                            TotalSalesAmount = x.TotalSalesAmount,
                            TotalSalesCount = x.SalesCount,
                            LastSaleDate = x.LastSaleDate,
                            RecencyWeightedScore = (x.SalesCount * 100.0) / (1 + x.DaysSinceLastSale)
                        })
                    .OrderByDescending(x => x.RecencyWeightedScore)
                    .ThenByDescending(x => x.LastSaleDate)
                    .ThenByDescending(x => x.TotalSalesCount)
                    .Take(limit)
                    .ToList();
                    return uniqueWeeksProductSales;

                case GroupByScope.Month:
                    var monthsProductSales = productSales.GroupBy(productSale => new
                    {
                        productSale.ProductId,
                        productSale.ItemName,
                        productSale.UnitPrice,
                        productSale.UnitsSold,
                        SaleDate = productSale.CreatedAt,
                        Month = productSale.CreatedAt.Month,
                        Year = productSale.CreatedAt.Year
                    }).Select(g => new
                    {
                        ProductId = g.Key.ProductId,
                        ItemName = g.Key.ItemName,
                        TotalSalesAmount = g.Sum(cp => cp.UnitPrice * cp.UnitsSold),
                        SalesCount = g.Count(),
                        LastSaleDate = g.Max(cp => cp.CreatedAt),
                        DaysSinceLastSale = (DateTime.Now - g.Max(cp => cp.CreatedAt)).TotalDays
                    }).ToList();
                    var uniqueMonthsProductSales = monthsProductSales.GroupBy(x => x.ProductId)
                        .Select(g => new
                        {
                            ProductId = g.Key,
                            ItemName = g.First().ItemName,
                            TotalSalesAmount = g.Sum(cp => cp.TotalSalesAmount),
                            SalesCount = g.Sum(cp => cp.SalesCount),
                            LastSaleDate = g.Max(cp => cp.LastSaleDate),
                            DaysSinceLastSale = (DateTime.Now - g.Max(cp => cp.LastSaleDate)).TotalDays
                        })
                        .Select(x => new TopSellingCategoryProduct
                        {
                            OnlineCategoryProductId = x.ProductId,
                            ProductName = x.ItemName,
                            TotalSalesAmount = x.TotalSalesAmount,
                            TotalSalesCount = x.SalesCount,
                            LastSaleDate = x.LastSaleDate,
                            RecencyWeightedScore = (x.SalesCount * 100.0) / (1 + x.DaysSinceLastSale)
                        })
                    .OrderByDescending(x => x.RecencyWeightedScore)
                    .ThenByDescending(x => x.LastSaleDate)
                    .ThenByDescending(x => x.TotalSalesCount)
                    .Take(limit)
                    .ToList();
                    return uniqueMonthsProductSales;
                default:
                    var productSalesByScope = orders.SelectMany(o => o.OrderProductItems
                        .Select(item => new
                        {
                            OnlineItemId = item.OnlineCategoryProductId,
                            ItemName = item.ProductName,
                            SaleDate = o.CreatedAt.Date,
                            Amount = item.UnitCost * item.Units
                        }))
                        .GroupBy(cp => new
                        {
                            Id = cp.OnlineItemId,
                            Period = GetTimeSpanByScope(cp.SaleDate, scope) // Get the time span based on the scope
                        })
                        .Select(g => new
                        {
                            OnlineItemId = g.Key.Id,
                            ItemName = g.First().ItemName,
                            TotalSalesAmount = g.Sum(cp => cp.Amount),
                            SalesCount = g.Count(),
                            LastSaleDate = g.Max(cp => cp.SaleDate),
                            DaysSinceLastSale = (DateTime.Now - g.Max(cp => cp.SaleDate)).TotalDays,
                            ScopeTimeSpan = g.Key.Period
                        }).Select(x => new TopSellingCategoryProduct
                        {
                            OnlineCategoryProductId = x.OnlineItemId,
                            ProductName = x.ItemName,
                            TotalSalesAmount = x.TotalSalesAmount,
                            TotalSalesCount = x.SalesCount,
                            LastSaleDate = x.LastSaleDate,
                            RecencyWeightedScore = (x.SalesCount * 100.0) / (1 + x.DaysSinceLastSale)
                        })
                        .OrderByDescending(x => x.RecencyWeightedScore)
                        .ThenByDescending(x => x.LastSaleDate)
                        .ThenByDescending(x => x.TotalSalesCount)
                        .Take(limit)
                        .ToList();
                    return productSalesByScope;
            }
        }

        public async Task<List<TopSellingCategory>> GetTopSellingCategoriesByScopeAsync(GroupByScope scope, DateTime? startDate = null, DateTime? endDate = null, int limit = 10)
        {
            Debug.WriteLine("Scope of work: " + scope);
            // Set the default range to the last 30 days
            var defaultEndDate = DateTime.UtcNow;
            var defaultStartDate = DateTime.UtcNow;

            // Get orders with product items
            var orders = await _context.CustomerOrders
                .Where(o => o.CreatedAt >= (startDate ?? defaultStartDate) &&
                o.CreatedAt <= (endDate ?? defaultEndDate))
                .Select(o => new { o.CreatedAt, o.OrderProductItems })
                .ToListAsync();
            var productSales = orders.SelectMany(o => o.OrderProductItems
                        .Select(item => item.OnlineCategoryProductId)
                        .Distinct()
                        .Select(productId => new
                        {
                            ProductId = productId,
                            SectionId = o.OrderProductItems.FirstOrDefault(item => item.OnlineCategoryProductId == productId)?.CategorySectionId ?? 0,
                            UnitPrice = o.OrderProductItems.FirstOrDefault(item => item.OnlineCategoryProductId == productId)?.UnitCost ?? 0,
                            UnitsSold = o.OrderProductItems.Where(cp => cp.OnlineCategoryProductId == productId).Count(),
                            CreatedAt = o.CreatedAt
                        })).ToList();
            foreach (var sale in productSales)
            {
                Debug.WriteLine(sale.Equals(null) ? "Sale is null" : $"ProductId: {sale.ProductId}," +
                    $" SectionId: {sale.SectionId}, UnitPrice: {sale.UnitPrice}, UnitsSold: {sale.UnitsSold}, CreatedAt: {sale.CreatedAt}");
            }

            var categorySections = await _context.CategorySections.ToDictionaryAsync(c => c.Id, c => c.CategorySectionName);
            switch (scope)
            {
                case GroupByScope.Day:
                    var todaysCategorySectionSales = productSales.GroupBy(productSale => new
                    {
                        productSale.ProductId,
                        productSale.SectionId,
                        productSale.UnitPrice,
                        productSale.UnitsSold,
                        productSale.CreatedAt

                    }).Select(g => new
                    {
                        CategorySectionId = g.Key.SectionId,
                        TotalSalesAmount = g.Sum(cp => cp.UnitPrice * cp.UnitsSold),
                        SalesCount = g.Count(),
                        LastSaleDate = g.Max(cp => cp.CreatedAt),
                        DaysSinceLastSale = (DateTime.Now - g.Max(cp => cp.CreatedAt)).TotalDays
                    }).ToList();
                    var todaysUniqueCategorySectionSales = todaysCategorySectionSales.GroupBy(x => x.CategorySectionId)
                        .Select(g => new
                        {
                            CategorySectionId = g.Key,
                            TotalSalesAmount = g.Sum(cp => cp.TotalSalesAmount),
                            SalesCount = g.Sum(cp => cp.SalesCount),
                            LastSaleDate = g.Max(cp => cp.LastSaleDate),
                            DaysSinceLastSale = (DateTime.Now - g.Max(cp => cp.LastSaleDate)).TotalDays
                        })
                    .Select(x => new TopSellingCategory
                    {
                        CategorySectionId = x.CategorySectionId,
                        CategoryName = categorySections.TryGetValue(x.CategorySectionId, out var name) ? name : "UnKnown Category",
                        TotalSalesAmount = x.TotalSalesAmount,
                        TotalSalesCount = x.SalesCount,
                        LastSaleDate = x.LastSaleDate,
                        RecencyWeightedScore = (x.SalesCount * 100.0) / (1 + x.DaysSinceLastSale),
                        PeriodTimeSpan = "Today"
                    })
                    .OrderByDescending(x => x.RecencyWeightedScore)
                        .ThenByDescending(x => x.LastSaleDate)
                        .ThenByDescending(x => x.TotalSalesCount)
                        .Take(limit)
                        .ToList();
                    return todaysUniqueCategorySectionSales;
                case GroupByScope.Week:
                    var weeksCategorySectionSales = productSales.GroupBy(productSale => new
                    {
                        productSale.ProductId,
                        productSale.SectionId,
                        SaleDate = productSale.CreatedAt,
                        Week = (productSale.CreatedAt.Day - 1) / 7 + 1 // Calculate week number in the month
                    }).Select(g => new
                    {
                        CategorySectionId = g.Key.SectionId,
                        TotalSalesAmount = g.Sum(cp => cp.UnitPrice * cp.UnitsSold),
                        SalesCount = g.Count(),
                        LastSaleDate = g.Key.SaleDate, // Assuming last sale date is today for weekly scope
                        DaysSinceLastSale = (DateTime.Now - g.Max(cp => cp.CreatedAt)).TotalDays
                    }).ToList();
                    var uniqueCategorySectionSales = weeksCategorySectionSales.GroupBy(x => x.CategorySectionId)
                        .Select(g => new
                        {
                            CategorySectionId = g.Key,
                            TotalSalesAmount = g.Sum(cp => cp.TotalSalesAmount),
                            SalesCount = g.Sum(cp => cp.SalesCount),
                            LastSaleDate = g.Max(cp => cp.LastSaleDate),
                            DaysSinceLastSale = (DateTime.Now - g.Max(cp => cp.LastSaleDate)).TotalDays
                        })
                        .Select(x => new TopSellingCategory
                        {
                            CategorySectionId = x.CategorySectionId,
                            CategoryName = categorySections.TryGetValue(x.CategorySectionId, out var name) ? name : "UnKnown Category",
                            TotalSalesAmount = x.TotalSalesAmount,
                            TotalSalesCount = x.SalesCount,
                            LastSaleDate = x.LastSaleDate,
                            RecencyWeightedScore = (x.SalesCount * 100.0) / (1 + x.DaysSinceLastSale),
                            PeriodTimeSpan = GetTimeSpanByScope(defaultEndDate, GroupByScope.Week) // Format week number as two digits
                        })
                    .OrderByDescending(x => x.RecencyWeightedScore)
                        .ThenByDescending(x => x.LastSaleDate)
                        .ThenByDescending(x => x.TotalSalesCount)
                        .Take(limit)
                        .ToList();
                    return uniqueCategorySectionSales;
                case GroupByScope.Month:
                    var monthsCategorySectionSales = productSales.GroupBy(productSale => new
                    {
                        productSale.ProductId,
                        productSale.SectionId,
                        SaleDate = productSale.CreatedAt,
                        Month = productSale.CreatedAt.Month,
                        Year = productSale.CreatedAt.Year
                    }).Select(g => new
                    {
                        CategorySectionId = g.Key.SectionId,
                        TotalSalesAmount = g.Sum(cp => cp.UnitPrice * cp.UnitsSold),
                        SalesCount = g.Count(),
                        LastSaleDate = g.Key.SaleDate, // Assuming last sale date is today for monthly scope
                        DaysSinceLastSale = (DateTime.Now - g.Max(cp => cp.CreatedAt)).TotalDays
                    }).ToList();

                    var uniqueMonthsCategorySectionSales = monthsCategorySectionSales.GroupBy(x => x.CategorySectionId)
                        .Select(g => new
                        {
                            CategorySectionId = g.Key,
                            TotalSalesAmount = g.Sum(cp => cp.TotalSalesAmount),
                            SalesCount = g.Sum(cp => cp.SalesCount),
                            LastSaleDate = g.Max(cp => cp.LastSaleDate),
                            DaysSinceLastSale = (DateTime.Now - g.Max(cp => cp.LastSaleDate)).TotalDays
                        })
                        .Select(x => new TopSellingCategory
                        {
                            CategorySectionId = x.CategorySectionId,
                            CategoryName = categorySections.TryGetValue(x.CategorySectionId, out var name) ? name : "UnKnown Category",
                            TotalSalesAmount = x.TotalSalesAmount,
                            TotalSalesCount = x.SalesCount,
                            LastSaleDate = x.LastSaleDate,
                            RecencyWeightedScore = (x.SalesCount * 100.0) / (1 + x.DaysSinceLastSale),
                            PeriodTimeSpan = GetTimeSpanByScope(defaultEndDate, GroupByScope.Month) // Format month as yyyy-MM
                        })
                        .OrderByDescending(x => x.RecencyWeightedScore)
                        .ThenByDescending(x => x.LastSaleDate)
                        .ThenByDescending(x => x.TotalSalesCount)
                        .Take(limit)
                        .ToList();
                    return uniqueMonthsCategorySectionSales;
                default:
                    var categorySales = orders.SelectMany(o => o.OrderProductItems
                        .Select(item => new
                        {
                            CategoryId = item.CategorySectionId,
                            Amount = item.UnitCost,
                            SoldUnits = item.Units,
                            SaleDate = o.CreatedAt.Date
                        }))
                        .GroupBy(cp => new
                        {
                            Id = cp.CategoryId,
                            Period = GetTimeSpanByScope(cp.SaleDate, scope) // Get the time span based on the scope
                        })
                        .Select(g => new
                        {
                            CategoryId = g.Key.Id,
                            TotalSalesAmount = g.Max(cp => cp.SoldUnits),
                            SalesCount = g.Count(),
                            LastSaleDate = g.Max(cp => cp.SaleDate),
                            DaysSinceLastSale = (DateTime.Now - g.Max(cp => cp.SaleDate)).TotalDays,
                            ScopeTimeSpan = g.Key.Period

                        }).Select(x => new TopSellingCategory
                        {
                            CategorySectionId = x.CategoryId,
                            CategoryName = categorySections.TryGetValue(x.CategoryId, out var name) ? name : "UnKnown Category",
                            TotalSalesAmount = x.TotalSalesAmount,
                            TotalSalesCount = x.SalesCount,
                            LastSaleDate = x.LastSaleDate,
                            RecencyWeightedScore = (x.SalesCount * 100.0) / (1 + x.DaysSinceLastSale)                            
                        })
                        .OrderByDescending(x => x.RecencyWeightedScore)
                        .ThenByDescending(x => x.LastSaleDate)
                        .ThenByDescending(x => x.TotalSalesCount)
                        .Take(limit)
                        .ToList();
                    return categorySales;
            }
        }

        public async Task<List<TopSellingCategory>> GetTopSellingCategoriesAsync(int limit = 10)
        {
            var orders = await _context.CustomerOrders                
                .ToListAsync();
            var categorySections = await _context.CategorySections.ToDictionaryAsync(c => c.Id, c => c.CategorySectionName);
            foreach(var categorySection in categorySections)
            {
                Debug.WriteLine($"Category Section ID: {categorySection.Key}, Name: {categorySection.Value}");
            }
            
            var categorySales = orders.SelectMany(o => o.OrderProductItems
                .Select(item => new
                {
                    CategoryId = item.CategorySectionId,
                    SalesAmount = item.UnitCost * item.Units,
                    SaleDate = o.CreatedAt
                }))
                .GroupBy(cp => cp.CategoryId)
                .Select(g => new
                {
                    CategoryId = g.Key,
                    TotalSalesAmount = g.Sum(cp => cp.SalesAmount),
                    SalesCount = g.Count(),
                    LastSaleDate = g.Max(cp => cp.SaleDate),
                    DaysSinceLastSale = (DateTime.Now - g.Max(cp => cp.SaleDate)).TotalDays
                }).Select(x => new TopSellingCategory
                {
                    CategorySectionId = x.CategoryId,
                    CategoryName = categorySections.TryGetValue(x.CategoryId, out var name) ? name : "UnKnown Category",
                    TotalSalesAmount = x.TotalSalesAmount,
                    TotalSalesCount = x.SalesCount,
                    LastSaleDate = x.LastSaleDate,
                    RecencyWeightedScore = (x.SalesCount * 100.0) / (1 + x.DaysSinceLastSale)
                })
                .OrderByDescending(x => x.RecencyWeightedScore)
                .ThenByDescending(x => x.LastSaleDate)
                .ThenByDescending(x => x.TotalSalesCount)
                .Take(limit)
                .ToList();
            return categorySales;
        }

        public async Task TrackInventoryChange(InventoryTracker inventoryTracker)
        {
            try
            {                
                await _context.InventoryTrackers.AddAsync(inventoryTracker);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw new Exception("Error tracking inventory change: " + ex.Message);
            }
        }

        public async Task<List<CategoryProduct>> GetCategoryProductsAsync()
        {
            var products = await _context.CategoryProducts.ToListAsync();
            return products;
        }

        public async Task<long> GetCategorySectionOnlineId(int categorySectionId)
        {
            var categorySection = await _context.CategorySections
                .AsNoTracking()
                .FirstOrDefaultAsync(cs => cs.Id == categorySectionId);
            return categorySection?.OnlineCategorySectionId ?? 0;
        }

        public async Task<bool> SoftDeleteCategoryProductAsync(int categoryProductId)
        {
            var categoryProduct = await _context.CategoryProducts.FindAsync(categoryProductId);
            if (categoryProduct == null)
            {
                return false; // Product not found
            }
            try
            {
                categoryProduct.IsDeleted = true; // Mark as deleted
                _context.CategoryProducts.Update(categoryProduct);
                await _context.SaveChangesAsync();
                return true; // Successfully marked as deleted
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false; // Error occurred while updating
            }
        }

        public List<StockItem> GroupStockItemsByStatusAsync(List<CategoryProduct> products, ProductInventoryStatus status)
        {
            switch (status)
            {
                case ProductInventoryStatus.Empty:
                    return products.Where(p => p.Units <= 0)
                        .Select(p => new StockItem
                        {
                            Id = p.Id,
                            ItemName = p.ProductName,
                            Quantity = p.Units,
                            Status = ProductInventoryStatus.Empty
                        }).ToList();
                case ProductInventoryStatus.Critical:
                    return products.Where(p => p.Units >= 1 && p.Units <= 5)
                        .Select(p => new StockItem
                        {
                            Id = p.Id,
                            ItemName = p.ProductName,
                            Quantity = p.Units,
                            Status = ProductInventoryStatus.Critical
                        }).ToList();
                case ProductInventoryStatus.Low:
                    return products.Where(p => p.Units > 5 && p.Units <= 10)
                        .Select(p => new StockItem
                        {
                            Id = p.Id,
                            ItemName = p.ProductName,
                            Quantity = p.Units,
                            Status = ProductInventoryStatus.Low
                        }).ToList();
            }
            return null;
        }

        public async Task<Store> GetDefaultUserAccountStore()
        {
            var store = await _context.Stores.OrderBy(s => s.Id).FirstOrDefaultAsync();
            return store;
        }

        public async Task<List<Category>> GetCachedDefaultCategories(IEnumerable<string> exstingCategorySectionNames)
        {
            var excludeSet = new HashSet<string>(exstingCategorySectionNames);
            var categories = await _context.Categories
                .AsNoTracking()    
                .Where(c => !excludeSet.Contains(c.CategoryName))
                .ToListAsync();
            return categories
                .GroupBy(c => c.CategoryName)                
                .Select(g => g.First()) // Get the first category for each unique OnlineCategoryId
                .ToList();
        }
        private string GetTimeSpanByScope(DateTime date, GroupByScope scope) {
            return scope switch
            {
                GroupByScope.Day => date.ToString("yyyy-MM-dd"),
                GroupByScope.Week => $"{date.Year}-W{GetIsoWeek(date)}",
                GroupByScope.Month => date.ToString("yyyy-MM"),
                GroupByScope.Year => date.ToString("yyyy"),
                _ => date.ToString("yyyy-MM-dd")

            };
        }

        private int GetIsoWeek(DateTime dateTime)
        {
            var day = (int) CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(dateTime);
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        public async Task<List<DefaultCategorySectionProduct>> GetCategorySectionDefaultProductsAsync(IEnumerable<long> existingCategorySectionIds)
        {
            var excludeSet = new HashSet<long>(existingCategorySectionIds);
            var defaultCategorySectionProducts = await _context.DefaultCategorySectionProducts
                .AsNoTracking()
                .Where(cp => !excludeSet.Contains(cp.CategorySectionId))
                .ToListAsync();
            return defaultCategorySectionProducts;
        }

        public async Task<List<DefaultCategorySectionProduct>> GetCategorySectionDefaultProductsByCategorySectionIdAsync(long categorySectionId)
        {
            var defaultCategorySectionProducts = await _context.DefaultCategorySectionProducts
                .AsNoTracking()
                .Where(cp => cp.CategorySectionId == categorySectionId)
                .ToListAsync();
            return defaultCategorySectionProducts;
        }

        public async Task<long> GetOnlineCategoryIdByName(string categoryName)
        {
            var category = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CategoryName == categoryName);
            return category?.OnlineCategoryId ?? 0;
        }

        public void Dispose()
        {
            _context.Dispose();
        }

     
    }
}
