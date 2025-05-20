using ChoziShop.Data.Enums;
using ChoziShop.Data.Models;
using ChoziShop.Data.Repository;
using ChoziShop.Data.Repository.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


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
            CategorySections = new Repository<CategorySection>(_context);
            CategoryProducts = new Repository<CategoryProduct>(_context);
            MerchantSessions = new Repository<MerchantSession>(_context);
            ShopOrders = new Repository<Order>(_context);
            PaymentAuths = new Repository<PaymentAuth>(_context);
            AirtelCollections = new Repository<AirtelPayCollection>(_context);
        }

        public IRepository<UnSyncedObject> UnSyncedObjects { get; }

        public IRepository<Merchant> Merchants { get; }
        public IRepository<Store> Stores { get; }
        public IRepository<CategorySection> CategorySections { get; }
        public IRepository<CategoryProduct> CategoryProducts { get; }
        public IRepository<MerchantSession> MerchantSessions { get; }
        public IRepository<Order> ShopOrders { get; }

        public IRepository<PaymentAuth> PaymentAuths { get; }
        public IRepository<AirtelPayCollection> AirtelCollections { get; }

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
                "c.currency, c.updated_at, c.item_code, c.units, c.unit_cost, c.measurement, c.remarks, c.tag, c.value_metric " +
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

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
