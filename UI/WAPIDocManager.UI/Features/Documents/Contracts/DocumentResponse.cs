using WAPIDocManager.UI.Features.Documents.Entities;
using WAPIDocManager.UI.Features.Documents.Models;

namespace WAPIDocManager.UI.Features.Documents.Contracts;

/// <remarks>
/// LE TRE FORME DEL DOCUMENTO NELLO SLICE (scelta deliberata, discussa e confermata: non unificarle senza motivo)
/// <list type="number">
///   <item><b>Contracts/</b> (questo file) – FORMA DI TRASPORTO: rispecchia il JSON del server, proprietà nullable,
///         nessuna validazione. Cambia solo se cambiano i DTO di WAPIDocument.</item>
///   <item><b>Entities/Document</b> – FORMA DI LETTURA usata da pagine, regole e permessi: collezioni mai null,
///         valori normalizzati. Costruita dall'operatore esplicito qui sotto.</item>
///   <item><b>Models/DocumentEditModel</b> – FORMA DI FORM: proprietà mutabili per il binding, regole in Validators/.
///         Torna verso le API con gli operatori di DocumentCreateRequest / DocumentUpdateRequest.</item>
/// </list>
/// Il prezzo è un mapping in più; il guadagno è che il JSON del server non arriva alle pagine e che la validazione
/// non sporca i modelli di lettura. Le tre forme vivono nello stesso slice, quindi una modifica si fa in una cartella.
/// </remarks>
/// <summary>
/// Risposta documento: stessa forma di DocumentReadResponse (senza Currency)
/// e di DocumentCreate/Update/UpdateStatus/GenerateFromResponse
/// </summary>
/// <remarks>
/// Un unico DTO per tutte le risposte (<c>WAPIDocument.Application/Dto/Document/*Response.cs</c>): <see cref="Currency"/>
/// è valorizzata solo nelle risposte di scrittura (DocumentReadResponse non la espone) e il client comunque non la usa.
/// Se i DTO cambiano sul server va aggiornato qui e nei test <c>DocumentApiServiceTests</c>.
/// </remarks>
public record DocumentResponse
{
    public string Id { get; init; } = string.Empty;
    public string? Number { get; init; }
    public DateTime Date { get; init; }
    public CustomerDto? Customer { get; init; }
    public string? Currency { get; init; }
    public DocumentType Type { get; init; }
    public DocumentStatus Status { get; init; }
    public IList<DocumentLineDto>? DocumentLines { get; init; }
    public decimal Total { get; init; }
    public IList<DocumentLinkDto>? LinkedDocuments { get; init; }

    /// <summary>
    /// Risposta API → documento del client. Le collezioni null diventano liste vuote e il cliente null un'istanza vuota.
    /// </summary>
    public static explicit operator Document(DocumentResponse response)
    {
        return new Document
        {
            Id = response.Id,
            Number = response.Number,
            Date = response.Date,
            Customer = response.Customer is null
                ? new Customer()
                : (Customer)response.Customer,
            Type = response.Type,
            Status = response.Status,
            Lines = response.DocumentLines?
                .Select(line => (DocumentLine)line)
                .ToList() ?? new List<DocumentLine>(),
            Total = response.Total,
            LinkedDocuments = response.LinkedDocuments?
                .Select(link => (DocumentLink)link)
                .ToList() ?? new List<DocumentLink>()
        };
    }
}
