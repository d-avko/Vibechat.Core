using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vibechat.DataLayer.Repositories.Specifications;

namespace Vibechat.DataLayer.Repositories
{
    public abstract class BaseRepository<T> where T: class
    {
        public ApplicationDbContext _dbContext { get; set; }

        public BaseRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public virtual async Task<T> GetByIdAsync(int id)
        {  
            return await _dbContext.Set<T>().FindAsync(id);
        }

        public async Task<IReadOnlyList<T>> ListAllAsync()
        {
            return await _dbContext.Set<T>().ToListAsync();
        }

        public async Task<IQueryable<T>> AsQuerableAsync(ISpecification<T> spec)
        {
            return ApplySpecification(spec).AsQueryable();
        }

        public async Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec)
        {
            return await ApplySpecification(spec).ToListAsync();
        }

        public async Task<int> CountAsync(ISpecification<T> spec)
        {
            return await ApplySpecification(spec).CountAsync();
        }
        
        public async Task<T> SingleOrDefaultAsync(ISpecification<T> spec)
        { 
            return await ApplySpecification(spec).SingleOrDefaultAsync();
        }

        public async Task ClearAsync()
        {
            _dbContext.Set<T>().RemoveRange(_dbContext.Set<T>());
        }

        public async Task<T> AddAsync(T entity)
        {
            _dbContext.Set<T>().Add(entity);
            return entity;
        }

        public virtual async Task UpdateAsync(T entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
        }

        public async Task DeleteAsync(T entity)
        {
            _dbContext.Set<T>().Remove(entity);
        }

        private IQueryable<T> ApplySpecification(ISpecification<T> spec)
        {
            return SpecificationEvaluator<T>.GetQuery(_dbContext.Set<T>().AsQueryable(), spec);
        }
    }
}
