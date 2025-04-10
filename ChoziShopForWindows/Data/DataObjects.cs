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
        }

        public IRepository<UnSyncedObject> UnSyncedObjects { get; }

        public IRepository<Merchant> Merchants { get; }
        public IRepository<Store> Stores { get; }

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

        public void Rollback()=>_context.ChangeTracker.Clear();

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
