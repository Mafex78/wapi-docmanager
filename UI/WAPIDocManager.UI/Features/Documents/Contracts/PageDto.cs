using WAPIDocManager.UI.Features.Documents.Entities;
using WAPIDocManager.UI.Features.Documents.Services;

namespace WAPIDocManager.UI.Features.Documents.Contracts;

/// <summary>
/// Pagina di risultati (<c>Shared.Application/Dto/PageDto.cs</c>).
/// </summary>
/// <remarks>
/// Nessun operatore di conversione: C# non permette a un tipo generico di dichiararne uno per una sua versione specifica
/// (es. <c>PageDto&lt;DocumentResponse&gt;</c>). La conversione in <c>Features/Documents/Entities/PagedResult</c> è in
/// <c>Services/DocumentApiService.FindPagedAsync</c>.
/// </remarks>
public record PageDto<T>
{
    public int PageSize { get; init; }
    public int CurrentPage { get; init; }
    public int TotalItems { get; init; }
    public int TotalPages { get; init; }
    public List<T>? Items { get; init; }
}
