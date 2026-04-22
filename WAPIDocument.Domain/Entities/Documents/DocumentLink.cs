namespace WAPIDocument.Domain.Entities.Documents;

public class DocumentLink
{
    public string TargetDocumentId { get; set; } =  string.Empty;
    public DocumentLinkType Type { get; set; }
    public DocumentType DocumentType { get; set; }
}