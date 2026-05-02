using WAPIDocument.Domain.Entities.Documents;

namespace WAPIDocument.Application.Dto.Document;

public record DocumentUpdateStatusResponse()
{
    public required string Id { get; init; }
    public required string? Number { get; init; }
    public required DateTime Date { get; init; }
    public required CustomerDto? Customer { get; init; }
    public required string Currency { get; init; }
    public required DocumentType Type { get; init; }
    public required DocumentStatus Status { get; init; }
    public required IList<DocumentLineDto>? DocumentLines  { get; init; }
    public required decimal Total { get; init; }
    public required IList<DocumentLinkDto> LinkedDocuments { get; init; }

    public static explicit operator DocumentUpdateStatusResponse(Domain.Entities.Documents.Document entity)
    {
        return new DocumentUpdateStatusResponse()
        {
            Id = entity.Id,
            Number = entity.Number,
            Date = entity.Date,
            Customer = entity.Customer is not null 
                ? (CustomerDto)entity.Customer 
                : new CustomerDto(),
            Currency = entity.Currency ?? string.Empty,
            Type = entity.Type,
            Status = entity.Status,
            DocumentLines = entity.DocumentLines?
                .Select(dl => (DocumentLineDto)dl)
                .ToList(),
            Total = entity.Total,
            LinkedDocuments = entity.LinkedDocuments
                .Select(ld => (DocumentLinkDto)ld)
                .ToList(),
        };
    }
}