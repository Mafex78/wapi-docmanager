using WAPIDocManager.UI.Features.Documents.Entities;

namespace WAPIDocManager.UI.Features.Documents.Contracts;

/// <summary>
/// Body di <c>POST api/v1/documents/{id}/generation</c> (<c>WAPIDocument.Application/Dto/Document/DocumentGenerateFromRequest.cs</c>).
/// </summary>
public record DocumentGenerateFromRequest
{
    /// <summary>Tipologia del documento da generare.</summary>
    public DocumentType DocumentType { get; init; }
}
