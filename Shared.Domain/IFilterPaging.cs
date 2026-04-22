namespace Shared.Domain;

public interface IFilterPaging
{
    /// <summary>
    /// Pagina da ricercare
    /// </summary>
    int Page { get; set; }
    
    /// <summary>
    /// Dimensione pagina
    /// </summary>
    int PageSize { get; set; }
    
    /// <summary>
    /// Ordinamento
    /// </summary>
    string? SortBy { get; set; }
    
    /// <summary>
    /// Direzione ordinamento
    /// </summary>
    string? SortDirection { get; set; }
}