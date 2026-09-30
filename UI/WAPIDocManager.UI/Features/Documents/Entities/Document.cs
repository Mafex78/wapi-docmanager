namespace WAPIDocManager.UI.Features.Documents.Entities;

/// Documento commerciale — preventivo, proforma, ordine di vendita — nella forma in cui l'applicazione lo legge.
/// È la stessa per tutte le risposte delle API, che siano una lettura, una creazione, una modifica, un cambio di
/// stato o una generazione: hanno tutte la medesima forma.
/// È un record immutabile: le pagine non lo modificano, lo sostituiscono con la risposta delle API o con una copia
/// che cambia i campi interessati.
/// Non contiene la valuta, che è unica per tutta l'applicazione.
public record Document
{
    /// Identificativo assegnato dal database, usato negli indirizzi delle API e delle pagine.
    public string Id { get; init; } = string.Empty;

    /// Numero logico assegnato dal server: è un GUID, e l'interfaccia ne mostra solo i primi caratteri.
    public string? Number { get; init; }

    /// Data del documento, fissata a mezzanotte UTC. Va mostrata senza conversione di fuso, altrimenti in certi
    /// fusi slitta al giorno prima.
    public DateTime Date { get; init; }

    /// Mai null: se le API non restituiscono il cliente, la conversione ne crea uno vuoto, così le pagine non
    /// devono difendersi.
    public Customer Customer { get; init; } = new();

    public DocumentType Type { get; init; }
    public DocumentStatus Status { get; init; }
    public IReadOnlyList<DocumentLine> Lines { get; init; } = new List<DocumentLine>();

    /// Totale calcolato dal server: la somma dei totali di riga, arrotondata a due decimali.
    public decimal Total { get; init; }

    public IReadOnlyList<DocumentLink> LinkedDocuments { get; init; } = new List<DocumentLink>();
}
