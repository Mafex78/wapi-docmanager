namespace WAPIDocManager.UI.Features.Documents.Entities;

/// Pagina di risultati di una ricerca paginata, nella forma usata dall'applicazione.
public record PagedResult<T>
{
    /// Elementi della pagina corrente, mai null.
    public IReadOnlyList<T> Items { get; init; } = new List<T>();

    /// Pagina corrente; la prima è la numero 1, come nelle API.
    public int CurrentPage { get; init; }

    public int PageSize { get; init; }

    /// Totale degli elementi che soddisfano il filtro, contando tutte le pagine.
    public int TotalItems { get; init; }

    public int TotalPages { get; init; }
}
