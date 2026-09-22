using WAPIDocManager.UI.Features.Documents.Contracts;

namespace WAPIDocManager.UI.Features.Documents.Entities;

/// <remarks>Forma di LETTURA: vedi le tre forme documentate in <c>Features/Documents/Contracts/DocumentResponse.cs</c>.</remarks>
/// <summary>
/// Documento commerciale (preventivo, proforma, ordine di vendita) come letto dalle API.
/// </summary>
/// <remarks>
/// <para>
/// Costruito con l'operatore esplicito di <c>Features/Documents/Contracts/DocumentResponse</c> a partire da tutte le risposte di WAPIDocument
/// (read, create, update, status, generation), che hanno la stessa forma.
/// </para>
/// <para>
/// Non contiene la valuta: vedi <see cref="DocumentCurrency"/>.
/// È un record immutabile: le pagine lo sostituiscono con la risposta delle API o con <c>with { ... }</c>.
/// </para>
/// </remarks>
public record Document
{
    /// <summary>ObjectId MongoDB (stringa): usato nelle route delle API e della UI.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Numero logico assegnato dal server: è un GUID (la UI ne mostra i primi 8 caratteri).</summary>
    public string? Number { get; init; }

    /// <summary>
    /// Data documento (mezzanotte UTC)
    /// </summary>
    /// <remarks>Va mostrata senza conversione di fuso orario (niente ToLocalTime), altrimenti può slittare di un giorno.</remarks>
    public DateTime Date { get; init; }

    /// <summary>Mai null: se le API non restituiscono il cliente il mapper crea un'istanza vuota.</summary>
    public Customer Customer { get; init; } = new();

    public DocumentType Type { get; init; }
    public DocumentStatus Status { get; init; }
    public IReadOnlyList<DocumentLine> Lines { get; init; } = new List<DocumentLine>();

    /// <summary>Totale calcolato dal server (somma dei totali riga arrotondata a 2 decimali).</summary>
    public decimal Total { get; init; }

    public IReadOnlyList<DocumentLink> LinkedDocuments { get; init; } = new List<DocumentLink>();
}
