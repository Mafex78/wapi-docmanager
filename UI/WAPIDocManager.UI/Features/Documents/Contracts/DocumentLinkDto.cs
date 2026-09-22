using WAPIDocManager.UI.Features.Documents.Entities;

namespace WAPIDocManager.UI.Features.Documents.Contracts;

/// <summary>
/// Link a un altro documento (<c>WAPIDocument.Application/Dto/Document/DocumentLinkDto.cs</c>).
/// </summary>
/// <remarks>Espone solo l'Id del documento collegato, non il tipo di link (System/User).</remarks>
public record DocumentLinkDto
{
    public string TargetDocumentId { get; init; } = string.Empty;

    /// <summary>
    /// DTO → link del client.
    /// </summary>
    public static explicit operator DocumentLink(DocumentLinkDto link)
    {
        return new DocumentLink
        {
            TargetDocumentId = link.TargetDocumentId
        };
    }
}
