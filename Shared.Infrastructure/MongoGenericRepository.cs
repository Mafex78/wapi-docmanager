using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Driver;
using Shared.Domain;

namespace Shared.Infrastructure;

public abstract class MongoGenericRepository<T> 
    : IGenericRepository<T, string>, IDisposable
    where T : class, IEntity<string>
{
    /// <summary>
    /// To detect redundant calls to Dispose
    /// </summary>
    private bool _disposed;
    
    private readonly IMongoCollection<T> _collection;
    
    private readonly IUnitOfWork _unitOfWork;

    public MongoGenericRepository(
        IMongoDatabase mongoDatabase,
        IUnitOfWork unitOfWork,
        string collectionName)
    {
        _collection = mongoDatabase.GetCollection<T>(collectionName);
        _unitOfWork = unitOfWork;
    }
    
    public async Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var session = GetSession();

        if (session != null)
        {
            return await (await _collection
                    .FindAsync(
                        session,
                        Builders<T>.Filter.Eq("_id", new ObjectId(id)),
                        null, 
                        cancellationToken))
                .SingleOrDefaultAsync(cancellationToken);
        } 
            
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
        var session = GetSession();

        if (session != null)
        {
            return await (await _collection
                    .FindAsync(
                        session,
                        filter, 
                        null, 
                        cancellationToken))
                .FirstOrDefaultAsync(cancellationToken);
        }
        
        return await (await _collection
            .FindAsync(
                filter, 
                null, 
                cancellationToken))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default)
    {
        var session = GetSession();
        
        T? entity;

        if (session != null)
        {
            entity = await (await _collection
                    .FindAsync(
                        session,
                        filter, 
                        null, 
                        cancellationToken))
                .FirstOrDefaultAsync(cancellationToken);
        }
        else
        {
            entity = await (await _collection
                    .FindAsync(
                        filter,
                        null,
                        cancellationToken))
                .FirstOrDefaultAsync(cancellationToken);
        }

        return entity != null;
    }

    public async Task<IPagedList<T>> FindPagedByFilterAsync(
        Expression<Func<T, bool>>? filter, 
        IFilterPaging paging, 
        CancellationToken cancellationToken = default)
    {
        var paged = new PagedList<T>();

        var page = paging.Page < 1 ? 1 : paging.Page;
        var pageSize = paging.PageSize;

        paged.CurrentPage = page;
        paged.PageSize = pageSize;

        var mongoFilter = filter != null
            ? Builders<T>.Filter.Where(filter)
            : Builders<T>.Filter.Empty;

        var session = GetSession();

        var find = session != null 
            ? _collection.Find(session, mongoFilter) 
            : _collection.Find(mongoFilter);
        
        if (!string.IsNullOrEmpty(paging.SortBy))
        {
            var sortDirection = paging.SortDirection?.ToLower() == "desc"
                ? Builders<T>.Sort.Descending(paging.SortBy)
                : Builders<T>.Sort.Ascending(paging.SortBy);

            find = find.Sort(sortDirection);
        }
        
        var totalItems = session is not null
            ? await _collection.CountDocumentsAsync(
                session,
                mongoFilter,
                cancellationToken: cancellationToken)
            : await _collection.CountDocumentsAsync(
            mongoFilter,
            cancellationToken: cancellationToken);

        var items = await find
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        paged.Items = items;
        paged.TotalItems = (int)totalItems;
        paged.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        return paged;

    }

    public async Task InsertAsync(T? item, CancellationToken cancellationToken = default)
    {
        if (item == null) 
            return;
        
        item.Version = 0;
        
        var session = GetSession();

        if (session != null)
        {
            await _collection.InsertOneAsync(
                session, 
                item, 
                new InsertOneOptions(), 
                cancellationToken);
        }
        else
        {
            await _collection.InsertOneAsync(
                item, 
                new InsertOneOptions(), 
                cancellationToken);
        }
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

        ReplaceOneResult result;
        var session = GetSession();
        
        if (session != null)
        {
            result = await _collection.ReplaceOneAsync(
                session, 
                filter, 
                item, 
                new ReplaceOptions(),
                cancellationToken: cancellationToken);
        }
        else
        {
            result = await _collection.ReplaceOneAsync(
                filter, 
                item, 
                new ReplaceOptions(),
                cancellationToken: cancellationToken);
        }

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
        var session = GetSession();
        
        if (session != null)
        {
            await _collection.DeleteOneAsync(
                session, 
                filter, 
                new DeleteOptions(), 
                cancellationToken);
        }
        else
        {
            await _collection.DeleteOneAsync(
                filter, 
                new DeleteOptions(), 
                cancellationToken);
        }
    }

    public async Task DeleteByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<T>.Filter.Eq("_id", new ObjectId(id));
        var session = GetSession();
        
        if (session != null)
        {
            await _collection.DeleteOneAsync(
                session, 
                filter, 
                new DeleteOptions(), 
                cancellationToken);
        }
        else
        {
            await _collection.DeleteOneAsync(
                filter, 
                new DeleteOptions(), 
                cancellationToken);
        }
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    /// <summary>
    /// Dispose
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        // Cleanup
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            //_collection.Database.Client.Dispose();
        }

        // Free unmanaged resources (unmanaged objects) and override a finalizer below.
        // Set large fields to null.

        _disposed = true;
    }
    
    private IClientSessionHandle? GetSession()
    {
        return (_unitOfWork as IMongoUnitOfWork)?.Session;
    }
}