using WAPIDocument.Domain.Entities.Documents;

namespace WAPIDocument.Application.Dto.Document;

public record DocumentAttachResponse
{
    public required IList<DocumentLinkDto> LinkedDocuments { get; init; }

    public static explicit operator DocumentAttachResponse(List<DocumentLink>? documentLinkEntities)
    {
        IList<DocumentLinkDto> documentLinksDto = documentLinkEntities is not null && documentLinkEntities.Any() 
            ? documentLinkEntities
            .Select(dl => (DocumentLinkDto)dl)
            .ToList()
            : new List<DocumentLinkDto>();
        
        return new DocumentAttachResponse()
        {
            LinkedDocuments = documentLinksDto
        };
    }
}