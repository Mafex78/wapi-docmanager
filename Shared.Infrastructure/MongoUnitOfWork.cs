using MongoDB.Driver;

namespace Shared.Infrastructure;

public class MongoUnitOfWork : IMongoUnitOfWork, IDisposable
{
    private readonly IMongoClient _mongoClient;
    private IClientSessionHandle? _session;
    public IClientSessionHandle? Session => _session;
    /// <summary>
    /// To detect redundant calls to Dispose
    /// </summary>
    private bool _disposed;
    
    public MongoUnitOfWork(IMongoClient mongoClient)
    {
        _mongoClient = mongoClient;
    }
    
    private async Task StartSessionAsync(CancellationToken cancellationToken = default)
    {
        if (_session != null)
        {
            throw new InvalidOperationException("Transaction is already in progress.");
        }

        var option = new ClientSessionOptions();
        option.DefaultTransactionOptions = new TransactionOptions(
            readConcern: ReadConcern.Majority, 
            writeConcern: WriteConcern.WMajority);
        
        _session = await _mongoClient
            .StartSessionAsync(option, cancellationToken);
    }
    
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        await StartSessionAsync(cancellationToken);
        ArgumentNullException.ThrowIfNull(_session);

        _session.StartTransaction();
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(_session);
        
        await _session.CommitTransactionAsync(cancellationToken);
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_session == null)
        {
            return;
        }
        
        await _session.AbortTransactionAsync(cancellationToken);
    }
    
    #region IDisposable
    
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
            // Dispose managed state (managed objects).
            if (_session != null)
            {
                _session.Dispose();
                _session = null;
            }

            //_collection.Database.Client.Dispose();
        }

        // Free unmanaged resources (unmanaged objects) and override a finalizer below.
        // Set large fields to null.

        _disposed = true;
    }
    
    #endregion
}