using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Shared.Domain;

namespace Shared.Infrastructure;

public abstract class GenericRepository<T, TKey> : IGenericRepository<T, TKey>
    where T : class, IEntity<TKey>
{
    private readonly DbContext _dbContext;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(DbContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = dbContext.Set<T>();
    }
    
    public async Task<T?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object?[] { id }, cancellationToken);
    }

    public async Task<T?> GetByFilterAsync(
        Expression<Func<T, bool>> filter,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(filter,  cancellationToken);
    }

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(filter, cancellationToken);
    }

    public async Task<IPagedList<T>> FindPagedByFilterAsync(
        Expression<Func<T, bool>>? filter, 
        IFilterPaging paging, 
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsQueryable();
        
        if (filter != null)
            query = query.Where(filter);

        return await query
            .PaginateAsync(paging.Page, paging.PageSize, paging.SortBy, paging.SortDirection);
    }

    public async Task InsertAsync(T? item, CancellationToken cancellationToken = default)
    {
        if (item == null) 
            return;
        
        await _dbSet.AddAsync(item,  cancellationToken);
    }

    public Task UpdateAsync(T? item, CancellationToken cancellationToken = default)
    {
        if (item == null)
            return Task.CompletedTask;

        _dbSet.Update(item);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(T? item, CancellationToken cancellationToken = default)
    {
        if (item == null)
            return Task.CompletedTask;

        _dbSet.Remove(item);
        return Task.CompletedTask;
    }

    public async Task DeleteByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id,  cancellationToken);
        await DeleteAsync(entity, cancellationToken);
    }
}