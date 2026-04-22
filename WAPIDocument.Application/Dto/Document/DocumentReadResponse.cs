using WAPIDocument.Domain.Entities.Documents;

namespace WAPIDocument.Application.Dto.Document;

public record DocumentReadResponse()
{
    public string Id { get; set; } = string.Empty;
    public string? Number { get; set; }
    public DateTime Date { get; set; }
    public CustomerDto? Customer { get; set; }
    public DocumentType Type { get; set; }
    public DocumentStatus Status { get; set; }
    public IList<DocumentLineDto>? DocumentLines { get; set; }
    public decimal Total { get; private set; } = 0M;
    public IList<DocumentLinkDto>? LinkedDocuments { get; set; }

    /// <summary>
    /// Converte l'entità sorgente dati in dto
    /// </summary>
    /// <param name="document"></param>
    /// <returns></returns>
    public static explicit operator DocumentReadResponse(Domain.Entities.Documents.Document document)
    {
        return new DocumentReadResponse()
        {
            Id = document.Id,
            Number = document.Number,
            Date = document.Date,
            Customer = document.Customer != null ? (CustomerDto)document.Customer : null,
            Type = document.Type,
            Status = document.Status,
            DocumentLines = document.DocumentLines != null
                ? document.DocumentLines.Select(x => (DocumentLineDto)x).ToList()
                : new List<DocumentLineDto>(),
            Total = document.Total,
            LinkedDocuments = document.LinkedDocuments
                .Select(x => (DocumentLinkDto)x)
                .ToList()
        };
    }
}