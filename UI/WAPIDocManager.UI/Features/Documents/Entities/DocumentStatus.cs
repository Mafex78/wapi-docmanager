namespace WAPIDocManager.UI.Features.Documents.Entities;

/// <summary>
/// Stato del ciclo di vita del documento.
/// </summary>
/// <remarks>
/// <para>
/// I valori numerici DEVONO restare identici a <c>WAPIDocument.Domain/Entities/Documents/DocumentStatus.cs</c>:
/// gli enum viaggiano come numeri e il body di <c>PUT api/v1/documents/{id}/status</c> è il solo valore numerico.
/// </para>
/// <para>
/// Transizioni ammesse: <see cref="DocumentRules.GetNextStatuses"/>.
/// Etichette localizzate: chiavi <c>DocumentStatus_{Valore}</c> (badge) e <c>StatusAction_{Valore}</c> (pulsanti di avanzamento).
/// Colore del badge: classe CSS <c>app-badge-{valore in minuscolo}</c> in <c>wwwroot/css/app.css</c>.
/// </para>
/// </remarks>
public enum DocumentStatus
{
    Draft = 0,      // incompleto
    Ready = 1,      // completo (tutti i campi obbligatori presenti)
    Sent = 2,       // inviato al cliente
    Approved = 3,   // approvato
    Rejected = 4    // rifiutato
}
