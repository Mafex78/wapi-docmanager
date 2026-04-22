using System.Linq.Expressions;

namespace Shared.Domain;

/// <summary>
/// Repository base con la definizione dei metodi comuni.
/// </summary>
/// <typeparam name="T">Tipologia della entita</typeparam>
/// <typeparam name="TKey">Tipo di chiave dell'entità</typeparam>
public interface IGenericRepository<T, TKey>
    where T : class, IEntity<TKey>
{
    Task<T?> GetByIdAsync(
        TKey id,
        CancellationToken cancellationToken = default);
    
    Task<T?> GetByFilterAsync(
        Expression<Func<T, bool>> filter,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        Expression<Func<T, bool>> filter,
        CancellationToken cancellationToken = default);

    Task<IPagedList<T>> FindPagedByFilterAsync(
        Expression<Func<T, bool>>? filter,
        IFilterPaging paging,
        CancellationToken cancellationToken = default);

    Task InsertAsync(T? item, CancellationToken cancellationToken = default);

    Task UpdateAsync(T? item);

    Task DeleteAsync(T? item);

    Task DeleteByIdAsync(TKey id, CancellationToken cancellationToken = default);
}