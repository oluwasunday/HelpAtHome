using HelpAtHome.Application.Interfaces.Repositories;
using HelpAtHome.Core.Entities;
using HelpAtHome.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace HelpAtHome.Application.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(AppDbContext context)
        { _context = context; _dbSet = context.Set<T>(); }

        public async Task<T?> GetByIdAsync(Guid id)
            => await _dbSet.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

        public async Task<IEnumerable<T>> GetAllAsync()
            => await _dbSet.Where(e => !e.IsDeleted).ToListAsync();

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.Where(predicate).ToListAsync();

        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.FirstOrDefaultAsync(predicate);

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.AnyAsync(predicate);

        public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
            => predicate == null
                ? await _dbSet.CountAsync()
                : await _dbSet.CountAsync(predicate);

        public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

        public void Update(T entity) { _dbSet.Update(entity); }

        public void SoftDelete(T entity)
        { entity.IsDeleted = true; entity.DeletedAt = DateTime.UtcNow; Update(entity); }

        /*public async Task<PagedResult<T>> GetPagedAsync(int page, int pageSize, Expression<Func<T, bool>>? filter = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet.Where(e => !e.IsDeleted);
            if (filter != null) query = query.Where(filter);
            foreach (var include in includes) query = query.Include(include);
            if (orderBy != null) query = orderBy(query);
            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedResult<T>(items, total, page, pageSize);
        }*/
    }

}
