namespace WAPIDocManager.UI.Features.Documents.ViewModels;

/// <summary>
/// Esito positivo dell'ultima azione sul dettaglio documento.
/// </summary>
/// <remarks>
/// Il ViewModel non conosce i testi: la pagina traduce il valore nella chiave corrispondente
/// (<c>Detail_StatusUpdated</c>, <c>Detail_Attached</c>) e lo azzera con <c>ClearNotification()</c>.
/// </remarks>
public enum DocumentDetailNotification
{
    None = 0,

    /// <summary>Stato del documento aggiornato (il nuovo stato è in <c>NotificationStatus</c>).</summary>
    StatusUpdated = 1,

    /// <summary>Documento collegato a un altro documento.</summary>
    Attached = 2
}
