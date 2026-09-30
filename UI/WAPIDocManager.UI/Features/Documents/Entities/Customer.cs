namespace WAPIDocManager.UI.Features.Documents.Entities;

/// Dati fiscali del cliente, conservati dentro il documento come fotografia al momento dell'emissione: non sono un
/// collegamento a un'anagrafica, quindi modificarli altrove non cambia i documenti già emessi.
/// Nessun campo è obbligatorio per creare o modificare il documento; ragione sociale e partita IVA lo diventano solo
/// per farlo avanzare oltre la bozza.
public record Customer
{
    /// Ragione sociale.
    public string? Name { get; init; }

    public string? Email { get; init; }

    /// Partita IVA.
    public string? VatNumber { get; init; }

    public string? Address { get; init; }
}
