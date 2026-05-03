using System.Linq.Expressions;
using FluentValidation;
using Shared.Application;
using Shared.Application.Dto;
using Shared.Domain;
using WAPIDocument.Application.Dto;
using WAPIDocument.Application.Dto.Document;
using WAPIDocument.Domain.Entities.Documents;
using WAPIDocument.Domain.Entities.TaxEntities;
using WAPIDocument.Domain.Repositories;

namespace WAPIDocument.Application.Services;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<DocumentCreateRequest> _documentCreateRequestValidator;
    private readonly IValidator<DocumentUpdateRequest> _documentUpdateRequestValidator;
    private readonly IValidator<DocumentChangeStatusContext> _documentChangeStatusValidator;
    private readonly IValidator<DocumentGenerateFromRequest> _documentGenerateFromRequestValidator;

    public DocumentService(
        IDocumentRepository documentRepository,
        IUnitOfWork unitOfWork,
        IValidator<DocumentCreateRequest> documentCreateRequestValidator,
        IValidator<DocumentUpdateRequest> documentUpdateRequestValidator,
        IValidator<DocumentChangeStatusContext> documentChangeStatusValidator,
        IValidator<DocumentGenerateFromRequest> documentGenerateFromRequestValidator)
    {
        _documentRepository = documentRepository;
        _unitOfWork = unitOfWork;
        _documentCreateRequestValidator = documentCreateRequestValidator;
        _documentUpdateRequestValidator = documentUpdateRequestValidator;
        _documentChangeStatusValidator = documentChangeStatusValidator;
        _documentGenerateFromRequestValidator = documentGenerateFromRequestValidator;
    }
    
    public async Task<DocumentCreateResponse> CreateAsync(
        DocumentCreateRequest model, 
        CancellationToken cancellationToken)
    {
        await _documentCreateRequestValidator.ValidateAndThrowAsync(
            model, 
            cancellationToken: cancellationToken);
        
        Document document = (Document)model;

        document.Create(model.Type);
        
        await _documentRepository.InsertAsync(document, cancellationToken);
        
        return (DocumentCreateResponse)document;
    }
    
    public async Task<DocumentReadResponse?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        Document? document = await _documentRepository.GetByIdAsync(id, cancellationToken);

        if (document == null)
        {
            throw new KeyNotFoundException();
        }
        
        return (DocumentReadResponse)document;
    }

    public async Task<PageDto<DocumentReadResponse>> FindPagedByFilterAsync(
        DocumentFindPagedByFilterRequest model, 
        CancellationToken cancellationToken)
    {
        var filter = GetFilter(model);

        IPagedList<Document> documents = await _documentRepository
            .FindPagedByFilterAsync(
                filter,
                model,
                cancellationToken);

        return new PageDto<DocumentReadResponse>()
        {
            PageSize = documents.PageSize,
            CurrentPage = documents.CurrentPage,
            TotalItems = documents.TotalItems,
            TotalPages = documents.TotalPages,
            Items = documents.Items?.Select(x => (DocumentReadResponse)x).ToList()
                    ?? new List<DocumentReadResponse>()
        };
    }

    public async Task<DocumentUpdateResponse> UpdateAsync(
        string id,
        DocumentUpdateRequest model, 
        CancellationToken cancellationToken)
    {
        await _documentUpdateRequestValidator.ValidateAndThrowAsync(
            model, 
            cancellationToken: cancellationToken);
        
        Document? document = await _documentRepository.GetByIdAsync(id, cancellationToken);

        if (document == null)
        {
            throw new KeyNotFoundException();
        }
        
        document.Update(
            model.Date,
            model.Currency,
            model.Customer is not null 
                ? (Customer)model.Customer 
                : null,
            model.DocumentLines is not null && model.DocumentLines.Any() 
                ? model.DocumentLines.Select(x => (DocumentLine)x).ToList() 
                : null);

        await _documentRepository.UpdateAsync(document, cancellationToken);
        return (DocumentUpdateResponse)document;
    }

    public async Task<DocumentUpdateStatusResponse> UpdateStatusAsync(string id, DocumentStatus newStatus, CancellationToken cancellationToken)
    {
        await _documentChangeStatusValidator.ValidateAndThrowAsync(
            new DocumentChangeStatusContext()
            {
                NewStatus = newStatus
            }, 
            cancellationToken: cancellationToken);
        
        Document? document = await _documentRepository.GetByIdAsync(id, cancellationToken);

        if (document == null)
        {
            throw new KeyNotFoundException();
        }

        document.UpdateStatus(newStatus);
        
        await _documentRepository.UpdateAsync(document, cancellationToken);
        return (DocumentUpdateStatusResponse)document;
    }
    
    public async Task DeleteByIdAsync(string id, CancellationToken cancellationToken)
    {
        Document? document = await _documentRepository.GetByIdAsync(id, cancellationToken);
        
        if (document == null)
        {
            throw new KeyNotFoundException();
        }   
        
        document.Delete();

        IList<Document> linkedDocuments = await GetLinkedDocumentsAsync(
            id, 
            cancellationToken);

        foreach (Document linkedDocument in linkedDocuments)
        {
            DocumentLink documentLinkToRemove = linkedDocument.LinkedDocuments
                .First(item => item.TargetDocumentId == id);
            
            linkedDocument.LinkedDocuments.Remove(documentLinkToRemove);
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        
        try
        {
            await _documentRepository.DeleteByIdAsync(id, cancellationToken);

            foreach (Document linkedDocument in linkedDocuments)
            {
                await _documentRepository.UpdateAsync(
                    linkedDocument,
                    cancellationToken);
            }
        
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private async Task<IList<Document>> GetLinkedDocumentsAsync(
        string id, 
        CancellationToken cancellationToken)
    {
        int pageSize = 20;
        FilterPagingDto filterPaging = new FilterPagingDto()
        {
            PageSize = pageSize,
            Page = 1,
        };
        
        Expression<Func<Document,bool>> filter = x => x.LinkedDocuments
            .Any(item => item.TargetDocumentId == id);

        IPagedList<Document> documents;
        
        do
        {
            documents = await _documentRepository
                .FindPagedByFilterAsync(
                    filter,
                    filterPaging,
                    cancellationToken);
            
            filterPaging.Page += 1;
            
        } while (documents.Items is not null && 
                 documents.Items.Count() == pageSize);
        
        return documents.Items?.ToList() ?? new List<Document>();
    }

    public async Task<DocumentGenerateFromResponse> GenerateFromAsync(
        string id,
        DocumentGenerateFromRequest model,
        CancellationToken cancellationToken)
    {
        await _documentGenerateFromRequestValidator.ValidateAndThrowAsync(
            model, 
            cancellationToken: cancellationToken);
        
        Document? documentSource = await _documentRepository.GetByIdAsync(id, cancellationToken);

        if (documentSource == null)
        {
            throw new KeyNotFoundException();
        }
        
        Document newDocumentToGenerate = documentSource.GenerateFrom(model.DocumentType);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        
        try
        {
            await _documentRepository.InsertAsync(newDocumentToGenerate, cancellationToken);
            documentSource.Attach(newDocumentToGenerate.Id, DocumentLinkType.System);
            await _documentRepository.UpdateAsync(documentSource, cancellationToken);
        
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        return (DocumentGenerateFromResponse)newDocumentToGenerate;
    }

    public async Task<DocumentAttachResponse> AttachAsync(string id, DocumentAttachRequest model, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(model.Id))
        {
            throw new ArgumentNullException("Attachment Id must be specified");
        }
        
        FilterPagingDto filterPaging = new FilterPagingDto()
        {
            PageSize = 2,
            Page = 1,
        };
        
        IPagedList<Document> documents = await _documentRepository
            .FindPagedByFilterAsync(
                filter: x => x.Id == id || x.Id == model.Id,
                filterPaging,
                cancellationToken);

        if (documents.Items is null ||
            documents.TotalItems < 2)
        {
            throw new KeyNotFoundException();
        }

        Document document = documents.Items.Where(x => x.Id == id).First();
        Document attachment = documents.Items.Where(x => x.Id == model.Id).First();
        
        document.Attach(attachment.Id, DocumentLinkType.User);
        attachment.Attach(document.Id, DocumentLinkType.User);
        
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        
        try
        {
            await _documentRepository.UpdateAsync(document, cancellationToken);
            await _documentRepository.UpdateAsync(attachment, cancellationToken);
        
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
        
        return (DocumentAttachResponse)document.LinkedDocuments;
    }
    
    #region Private methods
    
    private static Expression<Func<Document, bool>> GetFilter(DocumentFindPagedByFilterRequest model)
    {
        Expression<Func<Document, bool>> filter = ExpressionExtensions.True<Document>();

        if (!string.IsNullOrWhiteSpace(model.CustomerName))
        {
            filter = x => 
                x.Customer != null && 
                !string.IsNullOrWhiteSpace(x.Customer.Name) && 
                x.Customer.Name.ToLower().Contains(model.CustomerName.ToLower());
        }
        
        if (model.DocumentTypes.Any())
        {
            filter = filter.And(x => model.DocumentTypes.Contains(x.Type));
        }
        
        if(model.DocumentStatuses.Any())
        {
            filter = filter.And(x => model.DocumentStatuses.Contains(x.Status));
        }

        if (!string.IsNullOrWhiteSpace(model.PlainText))
        {
            bool isValidLookupDate = DateTime.TryParse(model.PlainText, out var lookupDate);
            DateTime utcDate = new DateTime(lookupDate.Year, lookupDate.Month, lookupDate.Day, 0, 0, 0, DateTimeKind.Utc);
            
            filter = filter.And(x => 
                (!string.IsNullOrWhiteSpace(x.Number) && x.Number.Contains(model.PlainText)) || 
                (isValidLookupDate && x.Date == utcDate));
        }

        return filter;
    }

    #endregion
}