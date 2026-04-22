using WAPIDocument.Domain.Entities.Documents;

namespace WAPIDocument.Application.Dto;

public record DocumentChangeStatusContext()
{
    public Domain.Entities.Documents.Document? Document { get; init; }
    public DocumentStatus NewStatus { get; init; }
}