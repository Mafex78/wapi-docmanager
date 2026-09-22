using System.Net.Http.Json;
using WAPIDocManager.UI.Features.Documents.Contracts;
using WAPIDocManager.UI.Features.Documents.Entities;
using WAPIDocManager.UI.Features.Documents.Models;
using WAPIDocManager.UI.Shared.Api;

namespace WAPIDocManager.UI.Features.Documents.Services;

/// <summary>
/// Integrazione con gli endpoint di WAPIDocument (api/v1/documents)
/// </summary>
/// <remarks>
/// <para>
/// Typed HttpClient con BaseAddress = <c>Api:DocumentBaseUrl</c> e BearerTokenHandler.
/// Controller di riferimento: <c>WAPIDocument/Controllers/DocumentsController.cs</c> (route <c>api/v{version}/documents</c>, versione 1.0).
/// </para>
/// <para>
/// Schema di ogni metodo: body costruito con l'operatore esplicito del contratto (es. <c>(DocumentCreateRequest)model</c>) →
/// chiamata HTTP → <see cref="ApiResponseReader"/> (errori in ApiException) → conversione nel modello del client
/// con l'operatore del contratto di risposta (es. <c>(Document)result</c>).
/// Documentazione dei singoli endpoint su <see cref="IDocumentService"/>.
/// </para>
/// </remarks>
public class DocumentApiService : IDocumentService
{
    /// <summary>Percorso relativo (senza slash iniziale, così si combina con la BaseAddress).</summary>
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

        // conversione qui e non in un operatore: PageDto<T> è generico (vedi Contracts/Common/PageDto)
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
        // il body è il valore numerico dell'enum ([FromBody] DocumentStatus)
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
