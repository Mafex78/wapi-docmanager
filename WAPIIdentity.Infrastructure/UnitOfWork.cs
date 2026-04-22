using Shared.Domain;

namespace WAPIIdentity.Infrastructure;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    /// <summary>
    /// To detect redundant calls to Dispose
    /// </summary>
    private bool _disposed;
    
    /// <summary>
    /// Context
    /// </summary>
    private readonly IdentityDbContext _dbContext;

    public UnitOfWork(IdentityDbContext context)
    {
        _dbContext = context;
        _disposed = false;
    }
        
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);

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
            _dbContext.Dispose();
        }

        // Free unmanaged resources (unmanaged objects) and override a finalizer below.
        // Set large fields to null.

        _disposed = true;
    }

    public async ValueTask DisposeAsync()
    {
        await _dbContext.DisposeAsync();
    }
}