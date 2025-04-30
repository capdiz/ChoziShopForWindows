using ChoziShop.Data.Models;
using ChoziShop.Data.Repository;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
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
            MerchantSessions = new Repository<MerchantSession>(_context);
        }

        public IRepository<UnSyncedObject> UnSyncedObjects { get; }

        public IRepository<Merchant> Merchants { get; }
        public IRepository<Store> Stores { get; }
        public IRepository<CategorySection> CategorySections { get; }
        public IRepository<MerchantSession> MerchantSessions { get; }

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

        public void Rollback()=>_context.ChangeTracker.Clear();

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
