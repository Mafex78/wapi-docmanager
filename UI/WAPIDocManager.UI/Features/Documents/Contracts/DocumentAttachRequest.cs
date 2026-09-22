using WAPIDocManager.UI.Features.Documents.Entities;

namespace WAPIDocManager.UI.Features.Documents.Contracts;

/// <summary>
/// Body di <c>POST api/v1/documents/{id}/attachments</c> (<c>WAPIDocument.Application/Dto/Document/DocumentAttachRequest.cs</c>).
/// </summary>
public record DocumentAttachRequest
{
    /// <summary>Id del documento da collegare a quello indicato nella route.</summary>
    public string Id { get; init; } = string.Empty;
}
