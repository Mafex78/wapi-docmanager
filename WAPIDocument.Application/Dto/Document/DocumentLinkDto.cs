using WAPIDocument.Domain.Entities.Documents;

namespace WAPIDocument.Application.Dto.Document;

public record DocumentLinkDto
{
    public string TargetDocumentId { get; set; } =  string.Empty;
    public DocumentType DocumentType { get; set; }

    public static explicit operator DocumentLinkDto(DocumentLink documentLink)
    {
        return new DocumentLinkDto
        {
            TargetDocumentId = documentLink.TargetDocumentId,
            DocumentType = documentLink.DocumentType
        };
    }
}