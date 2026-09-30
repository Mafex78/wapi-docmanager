using WAPIDocManager.UI.Features.Documents.Entities;

namespace WAPIDocManager.UI.Features.Documents.Services.Dto;

/// Body di POST api/v1/documents/{id}/generation.
public record DocumentGenerateFromRequest
{
    /// Tipologia del documento da generare.
    public DocumentType DocumentType { get; init; }
}
