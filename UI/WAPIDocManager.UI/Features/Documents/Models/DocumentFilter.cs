using WAPIDocManager.UI.Features.Documents.Entities;

namespace WAPIDocManager.UI.Features.Documents.Models;

/// Filtro della ricerca paginata dei documenti. I criteri si combinano in AND.
/// È una classe mutabile perché è legata direttamente al form dell'elenco, e la sua istanza viene conservata durante
/// la navigazione, così tornando all'elenco si ritrova la ricerca appena fatta.
public class DocumentFilter
{
    /// Limite imposto dal server: oltre questo valore la richiesta viene rifiutata.
    public const int MaxPageSize = 20;

    /// Tipologie da includere; vuoto significa tutte.
    public List<DocumentType> Types { get; set; } = new();

    /// Stati da includere; vuoto significa tutti.
    public List<DocumentStatus> Statuses { get; set; } = new();

    /// Ricerca per contenuto sulla ragione sociale, senza distinzione fra maiuscole e minuscole.
    public string? CustomerName { get; set; }

    /// Ricerca sul numero del documento, per contenuto, oppure sulla data, che invece deve corrispondere esatta.
    /// La data viene interpretata dal server secondo la propria cultura, quindi il formato anno-mese-giorno è il
    /// più sicuro.
    public string? PlainText { get; set; }

    /// Campo di ordinamento; null significa nessun ordinamento.
    public string? SortBy { get; set; } = DocumentSortFields.Date;

    public SortDirection SortDirection { get; set; } = SortDirection.Desc;

    /// Pagina richiesta; la prima è la numero 1.
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = MaxPageSize;
}
