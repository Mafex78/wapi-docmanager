using Shared.Application.Dto;
using WAPIDocument.Domain.Entities.Documents;

namespace WAPIDocument.Application.Dto.Document;

public record DocumentFindPagedByFilterRequest : FilterPagingDto
{
    public IList<DocumentType> DocumentTypes { get; set; } = new List<DocumentType>();
    public IList<DocumentStatus> DocumentStatuses { get; set; } = new List<DocumentStatus>();
    public string? CustomerName { get; set; }
}