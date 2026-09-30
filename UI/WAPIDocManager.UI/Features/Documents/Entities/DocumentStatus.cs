namespace WAPIDocManager.UI.Features.Documents.Entities;

/// Stato del ciclo di vita del documento.
/// I valori numerici DEVONO restare identici a quelli del server: gli enum viaggiano come numeri, e il corpo della
/// richiesta di cambio stato è il solo valore numerico.
/// Le etichette tradotte si ricavano dal nome del valore, con due chiavi per ciascuno: una per il contrassegno e una
/// per il pulsante che porta a quello stato. Il colore del contrassegno è a sua volta una classe CSS costruita dal
/// nome in minuscolo.
public enum DocumentStatus
{
    Draft = 0,      // incompleto
    Ready = 1,      // completo (tutti i campi obbligatori presenti)
    Sent = 2,       // inviato al cliente
    Approved = 3,   // approvato
    Rejected = 4    // rifiutato
}
