using System.Net.Http.Json;
using WAPIDocManager.UI.Features.Documents.Entities;
using WAPIDocManager.UI.Features.Documents.Models;
using WAPIDocManager.UI.Features.Documents.Services.Dto;
using WAPIDocManager.UI.Shared.Api;

namespace WAPIDocManager.UI.Features.Documents.Services;

/// Chiamate agli endpoint dei documenti.
/// Ogni metodo segue lo stesso schema: il corpo della richiesta si ottiene con un cast dal modello alla forma di
/// trasporto, si chiama il servizio, la lettura della risposta trasforma gli errori in eccezione, e infine un altro
/// cast riporta il risultato nella forma usata dall'applicazione.
/// Cosa fa ciascuna operazione è documentato sull'interfaccia, non qui.
public class DocumentApiService : IDocumentService
{
    /// Percorso relativo, senza slash iniziale, così da combinarsi con l'indirizzo base del client HTTP.
    public const string BasePath = "api/v1/documents";

    private readonly HttpClient _httpClient;

    public DocumentApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public async Task<Document> CreateAsync(DocumentEditModel model, CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
            BasePath,
            (DocumentCreateRequest)model,
            JsonDefaults.Options,
            cancellationToken);

        return await ReadDocumentAsync(response, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Document> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage response = await _httpClient.GetAsync(DocumentPath(id), cancellationToken);

        return await ReadDocumentAsync(response, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PagedResult<Document>> FindPagedAsync(DocumentFilter filter, CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage response = await _httpClient.GetAsync(
            $"{BasePath}?{DocumentQueryStringBuilder.Build(filter)}",
            cancellationToken);

        PageDto<DocumentResponse> page = await ApiResponseReader.ReadAsync<PageDto<DocumentResponse>>(response, cancellationToken);

        // conversione scritta qui e non come operatore: la pagina è un tipo generico, e un operatore di conversione
        // non può essere dichiarato fra due tipi generici come questi
        return new PagedResult<Document>
        {
            Items = page.Items?.Select(item => (Document)item).ToList() ?? new List<Document>(),
            CurrentPage = page.CurrentPage,
            PageSize = page.PageSize,
            TotalItems = page.TotalItems,
            TotalPages = page.TotalPages
        };
    }

    /// <inheritdoc />
    public async Task<Document> UpdateAsync(string id, DocumentEditModel model, CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage response = await _httpClient.PutAsJsonAsync(
            DocumentPath(id),
            (DocumentUpdateRequest)model,
            JsonDefaults.Options,
            cancellationToken);

        return await ReadDocumentAsync(response, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Document> UpdateStatusAsync(string id, DocumentStatus newStatus, CancellationToken cancellationToken = default)
    {
        // il corpo della richiesta è il solo valore numerico dello stato
        using HttpResponseMessage response = await _httpClient.PutAsJsonAsync(
            $"{DocumentPath(id)}/status",
            newStatus,
            JsonDefaults.Options,
            cancellationToken);

        return await ReadDocumentAsync(response, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        // risposta 204 senza body: si verifica solo lo status
        using HttpResponseMessage response = await _httpClient.DeleteAsync(DocumentPath(id), cancellationToken);

        await ApiResponseReader.EnsureSuccessAsync(response, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Document> GenerateFromAsync(string id, DocumentType targetType, CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
            $"{DocumentPath(id)}/generation",
            new DocumentGenerateFromRequest { DocumentType = targetType },
            JsonDefaults.Options,
            cancellationToken);

        return await ReadDocumentAsync(response, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DocumentLink>> AttachAsync(string id, string documentIdToAttach, CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
            $"{DocumentPath(id)}/attachments",
            new DocumentAttachRequest { Id = documentIdToAttach },
            JsonDefaults.Options,
            cancellationToken);

        DocumentAttachResponse result = await ApiResponseReader.ReadAsync<DocumentAttachResponse>(response, cancellationToken);

        return (List<DocumentLink>)result;
    }

    // l'Id viene sempre codificato: arriva da route e da input dell'utente
    private static string DocumentPath(string id)
    {
        return $"{BasePath}/{Uri.EscapeDataString(id)}";
    }

    // tutte le risposte documento (read, create, update, status, generation) hanno la stessa forma
    private static async Task<Document> ReadDocumentAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        DocumentResponse result = await ApiResponseReader.ReadAsync<DocumentResponse>(response, cancellationToken);

        return (Document)result;
    }
}
