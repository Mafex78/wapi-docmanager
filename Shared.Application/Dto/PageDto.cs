namespace Shared.Application.Dto;

public record PageDto<T>()
{
    /// <summary>
    /// Dimensione pagina
    /// </summary>
    public int PageSize { get; set; }
        
    /// <summary>
    /// Pagina corrente (prima = 1)
    /// </summary>
    public int CurrentPage { get; set; }
        
    /// <summary>
    /// Totali elementi che corrispondono alla query
    /// </summary>
    public int TotalItems { get; set; }
        
    /// <summary>
    /// Totale delle pagine
    /// </summary>
    public int TotalPages { get; set; }
        
    /// <summary>
    /// Elementi pagina corrente
    /// </summary>
    public List<T>? Items { get; set; }
}