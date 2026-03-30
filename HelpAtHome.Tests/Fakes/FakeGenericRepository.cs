using HelpAtHome.Application.Interfaces.Repositories;
using HelpAtHome.Core.Entities;
using System.Linq.Expressions;

namespace HelpAtHome.Tests.Fakes
{
    /// <summary>
    /// In-memory repository backed by a List&lt;T&gt;.
    /// Expressions are compiled and run against the list — no EF Core involved.
    /// </summary>
    public class FakeGenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        public readonly List<T> Data = new();

        public Task<T?> GetByIdAsync(Guid id)
            => Task.FromResult(Data.FirstOrDefault(e => e.Id == id && !e.IsDeleted));

        public Task<IEnumerable<T>> GetAllAsync()
            => Task.FromResult<IEnumerable<T>>(Data.Where(e => !e.IsDeleted).ToList());

        public Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
            => Task.FromResult<IEnumerable<T>>(Data.Where(predicate.Compile()).ToList());

        public Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
            => Task.FromResult(Data.FirstOrDefault(predicate.Compile()));

        public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
            => Task.FromResult(Data.Any(predicate.Compile()));

        public Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
            => Task.FromResult(predicate == null ? Data.Count : Data.Count(predicate.Compile()));

        public Task AddAsync(T entity)
        {
            Data.Add(entity);
            return Task.CompletedTask;
        }

        public void Update(T entity)
        {
            // Entity is already in the list by reference; no action needed.
        }

        public void SoftDelete(T entity)
        {
            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
        }
    }
}
