using WAPIDocManager.UI.Features.Documents.Entities;
using WAPIDocManager.UI.Features.Documents.Models;

namespace WAPIDocManager.UI.Features.Documents.Services;

/// Operazioni sui documenti, una per ciascun endpoint esposto dalle API.
/// Ruoli richiesti: la lettura è aperta a tutti i ruoli, tutto il resto solo a chi ha diritti di scrittura.
/// Ogni metodo solleva un'eccezione per le risposte non riuscite: 400 porta con sé il messaggio del server, per
/// esempio quando l'operazione non è ammessa nello stato corrente, e 404 significa documento inesistente.
public interface IDocumentService
{
    /// Crea il documento in bozza. La data la assegna il server, e la valuta è sempre la stessa.
    Task<Document> CreateAsync(DocumentEditModel model, CancellationToken cancellationToken = default);

    Task<Document> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// Ricerca con filtri, ordinamento e paginazione.
    Task<PagedResult<Document>> FindPagedAsync(DocumentFilter filter, CancellationToken cancellationToken = default);

    /// Salva le modifiche. È consentito solo finché il documento è in bozza o pronto, e in quest'ultimo caso il
    /// server rivalida l'intero documento. La tipologia non è modificabile e viene ignorata.
    Task<Document> UpdateAsync(string id, DocumentEditModel model, CancellationToken cancellationToken = default);

    /// Fa avanzare il documento allo stato indicato, che deve essere uno di quelli raggiungibili da quello corrente.
    Task<Document> UpdateStatusAsync(string id, DocumentStatus newStatus, CancellationToken cancellationToken = default);

    /// Elimina il documento, consentito solo finché è in bozza o pronto. Il server toglie anche i collegamenti che
    /// altri documenti avevano verso questo, in un'unica transazione.
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);

    /// Crea un nuovo documento in bozza copiando cliente e righe da quello indicato, e collegando i due.
    /// Restituisce il documento generato, non quello di partenza.
    Task<Document> GenerateFromAsync(string id, DocumentType targetType, CancellationToken cancellationToken = default);

    /// Collega due documenti a mano; il collegamento vale in entrambi i versi.
    /// Restituisce i collegamenti aggiornati del documento di partenza. Collegare due volte lo stesso documento
    /// viene rifiutato dal server.
    Task<IReadOnlyList<DocumentLink>> AttachAsync(string id, string documentIdToAttach, CancellationToken cancellationToken = default);
}
