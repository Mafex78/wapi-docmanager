using WAPIDocManager.UI.Features.Documents.Entities;
using WAPIDocManager.UI.Features.Documents.Services;

namespace WAPIDocManager.UI.Features.Documents.Contracts;

/// <summary>
/// Risposta del collegamento (<c>WAPIDocument.Application/Dto/Document/DocumentAttachResponse.cs</c>):
/// link aggiornati del documento indicato nella route.
/// </summary>
public record DocumentAttachResponse
{
    public IList<DocumentLinkDto>? LinkedDocuments { get; init; }

    /// <summary>
    /// Risposta → link del client (null → lista vuota).
    /// </summary>
    /// <remarks>
    /// Conversione verso <c>List</c> e non verso <c>IReadOnlyList</c>: C# non ammette operatori di conversione verso interfacce.
    /// La lista viene poi restituita come <c>IReadOnlyList</c> da <c>DocumentApiService.AttachAsync</c>.
    /// </remarks>
    public static explicit operator List<DocumentLink>(DocumentAttachResponse response)
    {
        return response.LinkedDocuments?
            .Select(link => (DocumentLink)link)
            .ToList() ?? new List<DocumentLink>();
    }
}
