using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vibechat.DataLayer.Repositories.Specifications;

namespace Vibechat.DataLayer.Repositories
{
    public interface IAsyncRepository<T> where T: class
    {
        Task<T> GetByIdAsync(int id);
        Task<IReadOnlyList<T>> ListAllAsync();
        Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec);
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<int> CountAsync(ISpecification<T> spec);

        Task<IQueryable<T>> AsQuerableAsync(ISpecification<T> spec);

        Task ClearAsync();
    }
}
