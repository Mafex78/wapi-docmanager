using WAPIDocument.Domain.Entities.Documents;

namespace WAPIDocument.Application.Dto.Document;

public record DocumentLineDto()
{
    public string? Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 0M;
    public decimal UnitPrice { get; set; } = 0M;
    public decimal Total { get; set; } = 0M;

    public static explicit operator DocumentLineDto(DocumentLine documentLine)
    {
        return new DocumentLineDto()
        {
            Description = documentLine.Description,
            Quantity = documentLine.Quantity,
            UnitPrice = documentLine.UnitPrice,
            Total = documentLine.Total
        };
    }
}