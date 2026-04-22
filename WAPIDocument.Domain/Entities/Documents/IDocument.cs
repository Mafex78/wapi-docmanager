using WAPIDocument.Domain.Entities.TaxEntities;

namespace WAPIDocument.Domain.Entities.Documents;

public interface IDocument
{
    string? Number { get; set; }
    DateTime Date { get; set; }
    Customer? Customer { get; set; }
    DocumentType Type { get; }
    DocumentStatus Status { get; }
    IList<DocumentLine>? DocumentLines { get; set; }
    public decimal Total  { get; }
    IList<DocumentLink> LinkedDocuments { get; }
}