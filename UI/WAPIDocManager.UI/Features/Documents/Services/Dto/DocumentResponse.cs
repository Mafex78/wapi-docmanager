using WAPIDocManager.UI.Features.Documents.Entities;

namespace WAPIDocManager.UI.Features.Documents.Services.Dto;

/// Risposta del server per un documento. È la stessa per ogni operazione: lettura, creazione, modifica, cambio di
/// stato e generazione restituiscono tutte questa forma.
/// LE TRE FORME DEL DOCUMENTO, scelta deliberata e confermata: non unificarle senza un motivo.
///   1. questa, la FORMA DI TRASPORTO: rispecchia il JSON del server, tutto può essere null, nessuna regola.
///      Cambia solo se cambia il server;
///   2. la FORMA DI LETTURA, quella che usano pagine, regole e permessi: collezioni mai null e valori normalizzati,
///      costruita dall'operatore qui sotto;
///   3. la FORMA DI FORM: proprietà mutabili da legare ai campi, con le proprie regole di validazione.
/// Il prezzo è una conversione in più; il guadagno è che il JSON del server non arriva mai alle pagine, e che la
/// validazione non sporca i modelli di lettura. Le tre forme vivono nella stessa funzionalità, quindi una modifica
/// si fa comunque in una cartella sola.
/// La valuta è valorizzata solo nelle risposte alle scritture, e il client non la usa comunque: la tratta come
/// fissa.
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

    /// Dalla risposta al documento dell'applicazione: le collezioni assenti diventano liste vuote e il cliente
    /// assente un'istanza vuota, così le pagine non devono difendersi dai null.
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
