namespace WAPIDocManager.UI.Features.Documents.Entities;

/// <summary>
/// Dati fiscali del cliente, embedded nel documento (snapshot al momento dell'emissione).
/// </summary>
/// <remarks>
/// Nessun campo è obbligatorio in creazione/modifica; <see cref="Name"/> e <see cref="VatNumber"/>
/// diventano obbligatori per portare il documento in stato Ready/Sent (vedi <see cref="DocumentRules.IsComplete"/>).
/// </remarks>
public record Customer
{
    /// <summary>Ragione sociale.</summary>
    public string? Name { get; init; }

    public string? Email { get; init; }

    /// <summary>Partita IVA.</summary>
    public string? VatNumber { get; init; }

    public string? Address { get; init; }
}
