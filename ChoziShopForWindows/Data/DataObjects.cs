using ChoziShop.Data.Models;
using ChoziShop.Data.Repository;
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
            Merchants = new Repository<Merchant>(_context);
        }

        public IRepository<UnSyncedObject> UnSyncedObjects { get; }

        public IRepository<Merchant> Merchants { get; }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Rollback()=>_context.ChangeTracker.Clear();

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
