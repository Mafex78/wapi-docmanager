namespace Shared.Domain;

/// <summary>
/// Helper per paginazione
/// </summary>
public class PagedList<T> : IPagedList<T>
    where T : class
{
    /// <summary>
    /// Dimensione della pagina
    /// </summary>
    public int PageSize { get; set; }
    
    /// <summary>
    /// Pagina corrente
    /// </summary>
    public int CurrentPage { get; set; }
    
    /// <summary>
    /// Elementi totali in tabella
    /// </summary>
    public int TotalItems { get; set; }
    
    /// <summary>
    /// Pagine totali
    /// </summary>
    public int TotalPages { get; set; }
    
    /// <summary>
    /// Elementi nella pagina corrente
    /// </summary>
    public IEnumerable<T>? Items { get; set; }
}