namespace Shared.Domain;

/// <summary>
/// Helper per paginazione
/// </summary>
public interface IPagedList<T>
    where T : class
{
    /// <summary>
    /// Dimensione della pagina
    /// </summary>
    int PageSize { get; set; }
    
    /// <summary>
    /// Pagina corrente
    /// </summary>
    int CurrentPage { get; set; }
    
    /// <summary>
    /// Elementi totali
    /// </summary>
    int TotalItems { get; set; }
    
    /// <summary>
    /// Pagine totali
    /// </summary>
    int TotalPages { get; set; }
    
    /// <summary>
    /// Elementi nella pagina corrente
    /// </summary>
    IEnumerable<T>? Items { get; set; }
}