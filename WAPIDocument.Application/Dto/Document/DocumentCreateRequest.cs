using WAPIDocument.Domain.Entities.Documents;
using WAPIDocument.Domain.Entities.TaxEntities;

namespace WAPIDocument.Application.Dto.Document;

public record DocumentCreateRequest
{
    public DocumentType Type { get; set; }
    public string? Currency { get; set; }
    public CustomerDto? Customer { get; set; }
    public IList<DocumentCreateUpdateRequestDocumentLine>? DocumentLines { get; set; } =  new List<DocumentCreateUpdateRequestDocumentLine>();
    
    public static explicit operator Domain.Entities.Documents.Document(DocumentCreateRequest model)
    {
        return new Domain.Entities.Documents.Document()
        {
            Currency = model.Currency,
            Customer = model.Customer != null
                ? new Customer()
                {
                    Name = model.Customer.Name,
                    Email = model.Customer.Email,
                    Address = model.Customer.Address,
                    VatNumber = model.Customer.VatNumber
                }
                : null,
            DocumentLines = model.DocumentLines != null
                ? model.DocumentLines.Select(x => new DocumentLine()
                {
                    Description = x.Description,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice,
                }).ToList()
                : new List<DocumentLine>(),
        };
    }
}