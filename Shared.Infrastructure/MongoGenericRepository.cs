using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Driver;
using Shared.Domain;

namespace Shared.Infrastructure;

public abstract class MongoGenericRepository<T> : IGenericRepository<T, string>
    where T : class, IEntity<string>
{
    private readonly IMongoCollection<T> _collection;

    public MongoGenericRepository(IMongoDatabase mongoDatabase, string collectionName)
    {
        _collection = mongoDatabase.GetCollection<T>(collectionName);
    }
    
    public async Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await (await _collection
            .FindAsync(
                Builders<T>.Filter.Eq("_id", new ObjectId(id)),
                null, 
                cancellationToken))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<T?> GetByFilterAsync(
        Expression<Func<T, bool>> filter,
        CancellationToken cancellationToken = default)
    {
        return await (await _collection
            .FindAsync(
                filter, 
                null, 
                cancellationToken))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default)
    {
        var match = await (await _collection
                .FindAsync(
                    filter, 
                    null, 
                    cancellationToken))
            .FirstOrDefaultAsync(cancellationToken);
        
        return match != null;
    }

    public async Task<IPagedList<T>> FindPagedByFilterAsync(
        Expression<Func<T, bool>>? filter, 
        IFilterPaging paging, 
        CancellationToken cancellationToken = default)
    {
        var query = _collection
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
        
        item.Version = 0;
        await _collection.InsertOneAsync(item, new InsertOneOptions(),  cancellationToken);
    }

    public async Task UpdateAsync(T? item, CancellationToken cancellationToken = default)
    {
        if (item == null)
            return;
        
        var oldVersion = item.Version;
        item.Version++;
        
        var filter = Builders<T>.Filter.And(
            Builders<T>.Filter.Eq(x => x.Id, item.Id),
            Builders<T>.Filter.Eq("Version", oldVersion));

        var result = await _collection.ReplaceOneAsync(filter, item, new ReplaceOptions(), cancellationToken: cancellationToken);
        
        if (result.MatchedCount == 0)
        {
            // Rollback the in-memory version increment so the caller can retry with correct state
            item.Version = oldVersion;
            throw new InvalidOperationException(
                $"Optimistic concurrency conflict on {typeof(T).Name} with id '{item.Id}'.");
        }
    }

    public async Task DeleteAsync(T? item, CancellationToken cancellationToken = default)
    {
        if (item == null)
            return;

        var filter = Builders<T>.Filter.Eq(x => x.Id, item.Id);
        await _collection.DeleteOneAsync(filter, new DeleteOptions(), cancellationToken);
    }

    public async Task DeleteByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<T>.Filter.Eq("_id", new ObjectId(id));
        await _collection.DeleteOneAsync(filter, new DeleteOptions(), cancellationToken);
    }
}