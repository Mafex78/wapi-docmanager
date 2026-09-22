using WAPIDocManager.UI.Features.Documents.Contracts;

namespace WAPIDocManager.UI.Features.Documents.Entities;

/// <summary>
/// Collegamento verso un altro documento.
/// </summary>
/// <remarks>
/// <c>DocumentLinkDto</c> delle API espone solo l'Id del documento collegato (non il tipo di link System/User),
/// quindi per mostrarne numero, tipo e stato la pagina di dettaglio esegue un GET per ogni link.
/// I link User sono bidirezionali; i link System sono creati dalla generazione (sorgente → generato e viceversa).
/// </remarks>
public record DocumentLink
{
    public string TargetDocumentId { get; init; } = string.Empty;
}
