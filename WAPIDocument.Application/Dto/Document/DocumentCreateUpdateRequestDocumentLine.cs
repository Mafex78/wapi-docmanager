using WAPIDocument.Domain.Entities.Documents;

namespace WAPIDocument.Application.Dto.Document;

public class DocumentCreateUpdateRequestDocumentLine
{
    public string? Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 0M;
    public decimal UnitPrice { get; set; } = 0M;
    
    public static explicit operator DocumentLine(DocumentCreateUpdateRequestDocumentLine model)
    {
        return new DocumentLine
        {
            Description = model.Description,
            Quantity = model.Quantity,
            UnitPrice = model.UnitPrice,
        };
    }
}