using WAPIDocManager.UI.Features.Documents.Contracts;
using WAPIDocManager.UI.Features.Documents.Services;

namespace WAPIDocManager.UI.Features.Documents.Entities;

/// <summary>
/// Pagina di risultati di una ricerca paginata.
/// </summary>
/// <remarks>
/// Equivalente lato client di <c>Shared.Application/Dto/PageDto.cs</c> (mappato da <c>Features/Documents/Services/DocumentApiService.FindPagedAsync</c>).
/// </remarks>
public record PagedResult<T>
{
    /// <summary>Elementi della pagina corrente (mai null).</summary>
    public IReadOnlyList<T> Items { get; init; } = new List<T>();

    /// <summary>
    /// Pagina corrente (prima = 1)
    /// </summary>
    public int CurrentPage { get; init; }

    public int PageSize { get; init; }

    /// <summary>Totale elementi che soddisfano il filtro (su tutte le pagine).</summary>
    public int TotalItems { get; init; }

    public int TotalPages { get; init; }
}
