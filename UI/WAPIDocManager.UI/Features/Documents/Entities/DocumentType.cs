namespace WAPIDocManager.UI.Features.Documents.Entities;

/// Tipologia del documento commerciale.
/// I valori numerici DEVONO restare identici a quelli del server: gli enum viaggiano come numeri, sia nei corpi JSON
/// sia nella query string. Le etichette tradotte si ricavano dal nome del valore.
public enum DocumentType
{
    Quote = 0,
    Proforma = 1,
    SalesOrder = 2
}
