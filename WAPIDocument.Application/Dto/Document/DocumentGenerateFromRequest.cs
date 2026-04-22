using WAPIDocument.Domain.Entities.Documents;

namespace WAPIDocument.Application.Dto.Document;

public record DocumentGenerateFromRequest(
    DocumentType DocumentType);