using WAPIDocManager.UI.Features.Documents.Entities;
using WAPIDocManager.UI.Features.Documents.Models;
using WAPIDocManager.UI.Shared.Api;

namespace WAPIDocManager.UI.Features.Documents.Services;

/// <summary>
/// Operazioni sui documenti: una per ogni endpoint di <c>WAPIDocument/Controllers/DocumentsController.cs</c>.
/// </summary>
/// <remarks>
/// <para>
/// Implementazione: <c>Features/Documents/Services/DocumentApiService</c> (typed HttpClient verso DocumentBaseUrl con BearerTokenHandler).
/// </para>
/// <para>
/// Ruoli richiesti dalle API: lettura (GET) Viewer, Editor, Admin; scrittura (tutto il resto) Editor, Admin.
/// Tutti i metodi sollevano <c>ApiException</c> per le risposte non 2xx
/// (400 con il messaggio del dominio, es. stato non valido; 404 documento inesistente).
/// </para>
/// </remarks>
public interface IDocumentService
{
    /// <summary>
    /// <c>POST api/v1/documents</c>: crea il documento in stato Draft con data odierna (assegnata dal server) e valuta EUR.
    /// </summary>
    Task<Document> CreateAsync(DocumentEditModel model, CancellationToken cancellationToken = default);

    /// <summary><c>GET api/v1/documents/{id}</c>.</summary>
    Task<Document> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>GET api/v1/documents</c> con filtri, ordinamento e paginazione in query string (PageSize massimo 20).
    /// </summary>
    Task<PagedResult<Document>> FindPagedAsync(DocumentFilter filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>PUT api/v1/documents/{id}</c>: consentito solo in Draft/Ready; in Ready il server rivalida il documento.
    /// </summary>
    /// <remarks>Il tipo del documento non è modificabile: <see cref="DocumentEditModel.Type"/> viene ignorato.</remarks>
    Task<Document> UpdateAsync(string id, DocumentEditModel model, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>PUT api/v1/documents/{id}/status</c>: avanza lo stato secondo <see cref="DocumentRules.GetNextStatuses"/>.
    /// </summary>
    Task<Document> UpdateStatusAsync(string id, DocumentStatus newStatus, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>DELETE api/v1/documents/{id}</c>: consentito solo in Draft/Ready; il server rimuove anche i link dai documenti collegati (transazione).
    /// </summary>
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>POST api/v1/documents/{id}/generation</c>: crea un nuovo documento Draft copiando cliente e righe, collegato al sorgente.
    /// </summary>
    /// <returns>Il documento generato (non il sorgente).</returns>
    Task<Document> GenerateFromAsync(string id, DocumentType targetType, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>POST api/v1/documents/{id}/attachments</c>: collega manualmente due documenti (link bidirezionale, transazione).
    /// </summary>
    /// <returns>I link aggiornati del documento <paramref name="id"/>. Collegare due volte lo stesso documento restituisce 400.</returns>
    Task<IReadOnlyList<DocumentLink>> AttachAsync(string id, string documentIdToAttach, CancellationToken cancellationToken = default);
}
