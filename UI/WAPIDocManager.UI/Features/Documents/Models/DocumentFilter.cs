using WAPIDocManager.UI.Features.Documents.Entities;
using WAPIDocManager.UI.Features.Documents.Services;
using WAPIDocManager.UI.Features.Documents;
using WAPIDocManager.UI.Shared.Validation;

namespace WAPIDocManager.UI.Features.Documents.Models;

/// <summary>
/// Filtro della ricerca paginata documenti (GET api/v1/documents)
/// </summary>
/// <remarks>
/// <para>
/// Corrisponde a <c>WAPIDocument.Application/Dto/Document/DocumentFindPagedByFilterRequest.cs</c>
/// (che eredita <c>Shared.Application/Dto/FilterPagingDto.cs</c>); la serializzazione in query string è in
/// <c>Features/Documents/Services/DocumentQueryStringBuilder</c>. I filtri si combinano in AND.
/// </para>
/// <para>
/// Classe mutabile: è legata direttamente al form della lista e conservata tra le navigazioni da <c>WAPIDocManager.UI/Features/Documents/DocumentListState</c> e usata dal ViewModel della lista.
/// </para>
/// </remarks>
public class DocumentFilter
{
    /// <summary>
    /// Limite imposto da FilterPagingDtoValidator
    /// </summary>
    /// <remarks>Oltre questo valore il server risponde 400.</remarks>
    public const int MaxPageSize = 20;

    /// <summary>Tipologie da includere (vuoto = tutte).</summary>
    public List<DocumentType> Types { get; set; } = new();

    /// <summary>Stati da includere (vuoto = tutti).</summary>
    public List<DocumentStatus> Statuses { get; set; } = new();

    /// <summary>Ricerca "contiene" case-insensitive sulla ragione sociale del cliente.</summary>
    public string? CustomerName { get; set; }

    /// <summary>
    /// Ricerca su numero documento (contains) o data (match esatto)
    /// </summary>
    /// <remarks>La data viene interpretata dal server con DateTime.TryParse (cultura del server): il formato ISO yyyy-MM-dd è il più sicuro.</remarks>
    public string? PlainText { get; set; }

    /// <summary>Campo di ordinamento: uno dei valori di <see cref="DocumentSortFields"/> (null = nessun ordinamento).</summary>
    public string? SortBy { get; set; } = DocumentSortFields.Date;

    public SortDirection SortDirection { get; set; } = SortDirection.Desc;

    /// <summary>Pagina richiesta (prima = 1).</summary>
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = MaxPageSize;
}
