using Shop.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shop.DAL.Repositories
{
    public interface IRepository<T> where T :
        class
        , IEntity
        , new()
    {

        IQueryable<T> Items { get; }
        Task<IEnumerable<T>?> GetAllAsync(CancellationToken cancel = default);
        Task<T?> GetAsync(int id, CancellationToken Cancel = default);
        Task<T> AddAsync(T item, CancellationToken Cancel = default);
        Task UpdateAsync(T item, CancellationToken Cancel = default);
        Task RemoveAsync(int id, CancellationToken Cancel = default);
    }
}
