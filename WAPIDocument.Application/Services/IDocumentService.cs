using Shared.Application.Dto;
using WAPIDocument.Application.Dto.Document;
using WAPIDocument.Domain.Entities.Documents;

namespace WAPIDocument.Application.Services;

public interface IDocumentService
{
    Task<DocumentCreateResponse> CreateAsync(DocumentCreateRequest model, CancellationToken cancellationToken);
    
    Task<DocumentReadResponse?> GetByIdAsync(string id, CancellationToken cancellationToken);
    
    Task<PageDto<DocumentReadResponse>> FindPagedByFilterAsync(
        DocumentFindPagedByFilterRequest filterPagingDto, 
        CancellationToken cancellationToken);
    
    Task<DocumentUpdateResponse> UpdateAsync(string id, DocumentUpdateRequest model, CancellationToken cancellationToken);
    
    Task<DocumentUpdateStatusResponse> UpdateStatusAsync(string id, DocumentStatus newStatus, CancellationToken cancellationToken);
    
    Task DeleteByIdAsync(string id, CancellationToken cancellationToken);
    Task<DocumentGenerateFromResponse> GenerateFromAsync(
        string id,
        DocumentGenerateFromRequest model,
        CancellationToken cancellationToken);

    Task<DocumentAttachResponse> AttachAsync(string id, DocumentAttachRequest model, CancellationToken cancellationToken);
}