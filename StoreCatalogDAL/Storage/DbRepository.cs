using Microsoft.EntityFrameworkCore;
using StoreCatalogDAL.Model;

namespace StoreCatalogDAL.Storage
{
    internal class DbRepository<T>(StoreDbContext db) : IRepository<T> where T :
         class
        , IEntity
        , new()
    {
        private readonly DbSet<T> _set = db.Set<T>();
        public bool AutoSaveChanges { get; set; } = true;
        public virtual IQueryable<T> Items => _set;
        public T? Get(int id) => Items.SingleOrDefault(item => item.Id == id);
        public async Task<T?> GetAsync(int id, CancellationToken cancel = default) => await Items
           .SingleOrDefaultAsync(item => item.Id == id, cancel)
           .ConfigureAwait(false);
        public T Add(T item)
        {
            ArgumentNullException.ThrowIfNull(item);
            db.Entry(item).State = EntityState.Added;
            if (AutoSaveChanges)
                db.SaveChanges();
            return item;
        }
        public async Task<T> AddAsync(T item, CancellationToken cancel = default)
        {
            ArgumentNullException.ThrowIfNull(item);
            db.Entry(item).State = EntityState.Added;
            if (AutoSaveChanges)
                await db.SaveChangesAsync(cancel).ConfigureAwait(false);
            return item;
        }
        public void Update(T item)
        {
            ArgumentNullException.ThrowIfNull(item);
            db.Entry(item).State = EntityState.Modified;
            if (AutoSaveChanges)
                db.SaveChanges();
        }
        public async Task UpdateAsync(T item, CancellationToken cancel = default)
        {
            ArgumentNullException.ThrowIfNull(item);
            db.Entry(item).State = EntityState.Modified;
            if (AutoSaveChanges)
                await db.SaveChangesAsync(cancel).ConfigureAwait(false);
        }
        public void Remove(int id)
        {
            var item = _set.Local.FirstOrDefault(i => i.Id == id) ?? new T { Id = id };
            db.Remove(item);
            if (AutoSaveChanges)
                db.SaveChanges();
        }
        public async Task RemoveAsync(int id, CancellationToken cancel = default)
        {
            db.Remove(new T { Id = id });
            if (AutoSaveChanges)
                await db.SaveChangesAsync(cancel).ConfigureAwait(false);
        }
        public async Task<IEnumerable<T>?> GetAllAsync(CancellationToken cancel = default) => await _set.ToListAsync(cancel).ConfigureAwait(false);
    }
}
