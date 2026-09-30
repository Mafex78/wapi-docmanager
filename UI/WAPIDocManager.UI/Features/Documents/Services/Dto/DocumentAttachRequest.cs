namespace WAPIDocManager.UI.Features.Documents.Services.Dto;

/// Body di POST api/v1/documents/{id}/attachments.
public record DocumentAttachRequest
{
    /// Identificativo del documento da collegare a quello indicato nell'indirizzo.
    public string Id { get; init; } = string.Empty;
}
