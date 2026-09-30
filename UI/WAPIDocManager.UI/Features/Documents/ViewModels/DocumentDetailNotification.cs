namespace WAPIDocManager.UI.Features.Documents.ViewModels;

/// Esito positivo dell'ultima azione sul dettaglio del documento.
/// Il ViewModel non conosce i testi mostrati all'utente: restituisce che cosa è successo, e la pagina decide come
/// dirlo e quando toglierlo. È la stessa ragione per cui la traduzione non entra nei ViewModel.
public enum DocumentDetailNotification
{
    None = 0,

    /// Stato del documento aggiornato; qual è il nuovo stato lo dice una proprietà a parte.
    StatusUpdated = 1,

    /// Documento collegato a un altro documento.
    Attached = 2
}
