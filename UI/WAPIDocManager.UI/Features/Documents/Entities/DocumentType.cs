using WAPIDocManager.UI.Shared.Localization;

namespace WAPIDocManager.UI.Features.Documents.Entities;

/// <summary>
/// Tipologia del documento commerciale.
/// </summary>
/// <remarks>
/// I valori numerici DEVONO restare identici a <c>WAPIDocument.Domain/Entities/Documents/DocumentType.cs</c>:
/// le API serializzano gli enum come numeri (nessun JsonStringEnumConverter), sia nei body JSON sia in query string.
/// Etichette localizzate: chiave risorsa <c>DocumentType_{Valore}</c> (vedi <c>Shared/Localization/LocalizerExtensions.cs</c> nel progetto UI).
/// </remarks>
public enum DocumentType
{
    Quote = 0,
    Proforma = 1,
    SalesOrder = 2
}
