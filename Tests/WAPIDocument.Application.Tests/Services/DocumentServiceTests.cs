using System.Linq.Expressions;
using FluentValidation;
using Moq;
using Shared.Domain;
using WAPIDocument.Application.Dto;
using WAPIDocument.Application.Dto.Document;
using WAPIDocument.Application.Services;
using WAPIDocument.Domain.Entities.Documents;
using WAPIDocument.Domain.Entities.TaxEntities;
using WAPIDocument.Domain.Repositories;
using Xunit;
using DocumentLine = WAPIDocument.Domain.Entities.Documents.DocumentLine;

namespace WAPIDocument.Application.Tests.Services;

public class DocumentServiceTests
{
    private readonly Mock<IDocumentRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IValidator<DocumentCreateRequest>> _documentCreateRequestValidator = new();
    private readonly Mock<IValidator<DocumentUpdateRequest>> _documentUpdateRequestValidator = new();
    private readonly Mock<IValidator<DocumentChangeStatusContext>> _documentChangeStatusValidator = new();
    private readonly Mock<IValidator<DocumentGenerateFromRequest>> _documentGenerateFromRequestValidator = new();

    public DocumentServiceTests()
    {
        // Validatori di default configurati come "successo" per tutti i test,
        // così ci si  sulla logica del service.
        _documentCreateRequestValidator
            .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _documentUpdateRequestValidator
            .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _documentChangeStatusValidator
            .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _documentGenerateFromRequestValidator
            .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
    }
    
    #region CreateAsync
    
    [Fact]
    public async Task DocumentService_CreateAsync_Ok()
    {
        _repository
            .Setup(r => r.InsertAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();
        var result = await service.CreateAsync(BuildCreateRequest(), CancellationToken.None);

        Assert.NotNull(result);
    }
    
    #endregion
    
    #region GetByIdAsync
    
    [Fact]
    public async Task DocumentService_GetByIdAsync_Ko_NotFound()
    {
        _repository
            .Setup(r => r.GetByIdAsync("x", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetByIdAsync("x", CancellationToken.None));
    }

    [Fact]
    public async Task DocumentService_GetByIdAsync_Ok()
    {
        var document = new Document
        {
            Id = "1",
            Number = "N1",
            Date = DateTime.UtcNow,
            Currency = "EUR",
            Customer = new Customer
            {
                Name = "c"
            },
            DocumentLines = new List<DocumentLine>
            {
                new() { Description = "A", Quantity = 1, UnitPrice = 10 }
            }
        };
        _repository
            .Setup(r => r.GetByIdAsync("1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        var service = CreateService();
        var result = await service.GetByIdAsync("1", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("1", result.Id);
        Assert.Equal("N1", result.Number);
        Assert.Equal(10M, result.Total);
        Assert.NotNull(result.Customer);
        Assert.Single(result.DocumentLines!);
    }
    
    #endregion

    #region FindPagedByFilterAsync
    
    [Fact]
    public async Task DocumentService_FindPagedByFilterAsync_Ok_ReturnEmpty()
    {
        _repository
            .Setup(r => r.FindPagedByFilterAsync(
                It.IsAny<Expression<Func<Document, bool>>?>(),
                It.IsAny<IFilterPaging>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<Document> { Items = new List<Document>(), PageSize = 10, CurrentPage = 1 });

        var service = CreateService();
        var result = await service.FindPagedByFilterAsync(new DocumentFindPagedByFilterRequest { PlainText = null }, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result.Items!);
    }

    [Fact]
    public async Task DocumentService_FindPagedByFilterAsync_Ok_ReturnWithData()
    {
        _repository
            .Setup(r => r.FindPagedByFilterAsync(
                It.IsAny<Expression<Func<Document, bool>>?>(),
                It.IsAny<IFilterPaging>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<Document>
            {
                Items = new List<Document>
                {
                    new() { Id = "1", Number = "Q-2026", Date = new DateTime(2026, 4, 19, 0, 0, 0, DateTimeKind.Utc) }
                },
                PageSize = 10,
                CurrentPage = 1,
                TotalItems = 1,
                TotalPages = 1
            });

        var service = CreateService();
        var result = await service.FindPagedByFilterAsync(
            new DocumentFindPagedByFilterRequest { PlainText = "2026" }, 
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Single(result.Items!);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(1, result.CurrentPage);
        Assert.Equal(1, result.TotalItems);
        Assert.Equal(1, result.TotalPages);
    }
    
    #endregion
    
    #region UpdateAsync

    [Fact]
    public async Task DocumentService_UpdateAsync_Ko_NotFound()
    {
        _repository
            .Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);
        
        var service = CreateService();

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.UpdateAsync("missing", new DocumentUpdateRequest(), CancellationToken.None));
    }

    [Fact]
    public async Task DocumentService_UpdateAsync_Ok()
    {
        var existing = new Document
        {
            Id = "1",
            Number = "N",
            Currency = "EUR",
        };
        _repository.Setup(r => r.GetByIdAsync("1", It.IsAny<CancellationToken>())).ReturnsAsync(existing);

        _repository
            .Setup(r => r.UpdateAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        var newDate = new DateTime(2025, 5, 1, 0, 0, 0, DateTimeKind.Utc);
        var request = new DocumentUpdateRequest
        {
            Date = newDate,
            Currency = "USD",
            Customer = new CustomerDto
            {
                Name = "Customer"
            },
            DocumentLines = new List<DocumentCreateUpdateRequestDocumentLine>
            {
                new() { Description = "A", Quantity = 2, UnitPrice = 5 }
            }
        };

        await service.UpdateAsync("1", request, CancellationToken.None);
    }
    
    [Fact]
    public async Task DocumentService_UpdateAsync_Ko_DocumentNotEditable()
    {
        // Un documento già in stato Sent non deve poter essere aggiornato:
        // l'entità del dominio solleva InvalidOperationException.
        var document = CreateDocumentStatus(DocumentStatus.Sent);
        
        _repository
            .Setup(r => r.GetByIdAsync("1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        var service = CreateService();

        var request = new DocumentUpdateRequest
        {
            Date = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc),
            Currency = "USD",
            Customer = new CustomerDto
            {
                Name = "Customer"
            },
            DocumentLines = new List<DocumentCreateUpdateRequestDocumentLine>
            {
                new() { Description = "A", Quantity = 1, UnitPrice = 5 }
            }
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateAsync("1", request, CancellationToken.None));

        _repository.Verify(r => r.UpdateAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task DocumentService_UpdateStatusAsync_Ko_MissingLines()
    {
        var document = CreateValidDraftDocument();
        
        // Elimina tutte le linee documento
        // per fare in modo che la transizione
        // a Ready fallisca
        document.DocumentLines = new List<DocumentLine>();
        
        _repository.Setup(r => r.GetByIdAsync("1", It.IsAny<CancellationToken>())).ReturnsAsync(document);

        var service = CreateService();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateStatusAsync("1", DocumentStatus.Ready, CancellationToken.None));

        Assert.Contains("lines", ex.Message, StringComparison.OrdinalIgnoreCase);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task DocumentService_UpdateStatusAsync_Ko_InvalidTargetStatus()
    {
        var document = CreateValidDraftDocument();
        _repository.Setup(r => r.GetByIdAsync("1", It.IsAny<CancellationToken>())).ReturnsAsync(document);

        var service = CreateService();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateStatusAsync("1", DocumentStatus.Sent, CancellationToken.None));

        Assert.Equal(DocumentStatus.Draft, document.Status);
        _repository.Verify(r => r.UpdateAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
    
    #endregion
    
    #region DeleteByIdAsync

    [Fact]
    public async Task DocumentService_DeleteByIdAsync_Ok()
    {
        var source = CreateValidDraftDocument("abc");
        _repository
            .Setup(r => r.GetByIdAsync("abc", It.IsAny<CancellationToken>()))
            .ReturnsAsync(source)
            .Verifiable();
        
        _repository
            .Setup(r => r.DeleteByIdAsync("abc", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var service = CreateService();
        await service.DeleteByIdAsync("abc", CancellationToken.None);

        _repository.Verify(r => r.DeleteByIdAsync("abc", It.IsAny<CancellationToken>()), Times.Once);
    }
    
    #endregion
    
    #region GenerateFromAsync
    
    [Fact]
    public async Task DocumentService_GenerateFromAsync_Ok_ReturnsNewId()
    {
        var source = CreateValidDraftDocument("123");
        _repository.Setup(r => r.GetByIdAsync("123", It.IsAny<CancellationToken>())).ReturnsAsync(source);

        _repository
            .Setup(r => r.InsertAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        var result = await service.GenerateFromAsync(
            "123",
            new DocumentGenerateFromRequest(DocumentType.Proforma),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotEqual("123", result.Id);

        // Il documento sorgente non deve essere stato modificato
        Assert.Equal("123", source.Id);
    }
    
    [Fact]
    public async Task DocumentService_GenerateFromAsync_Ko_ValidationFails()
    {
        var failingValidator = new Mock<IValidator<DocumentGenerateFromRequest>>();
        failingValidator
            .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(new[]
            {
                new FluentValidation.Results.ValidationFailure("DocumentType", "invalid")
            }));

        var service = new DocumentService(
            _repository.Object,
            _documentCreateRequestValidator.Object,
            _documentUpdateRequestValidator.Object,
            _documentChangeStatusValidator.Object,
            failingValidator.Object);

        await Assert.ThrowsAsync<ValidationException>(
            () => service.GenerateFromAsync(
                "any",
                new DocumentGenerateFromRequest(DocumentType.Quote),
                CancellationToken.None));

        _repository.Verify(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _repository.Verify(r => r.InsertAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
    
    #endregion
    
    #region Private methods
    
    private DocumentService CreateService() => new(
        _repository.Object, 
        _documentCreateRequestValidator.Object,
        _documentUpdateRequestValidator.Object,
        _documentChangeStatusValidator.Object,
        _documentGenerateFromRequestValidator.Object);

    private static DocumentCreateRequest BuildCreateRequest() => new()
    {
        Currency = "USD",
        Customer = new CustomerDto
        {
            Name = "CustomerName"
        },
        DocumentLines = new List<DocumentCreateUpdateRequestDocumentLine>
        {
            new() { Description = "Item", Quantity = 2, UnitPrice = 10 }
        }
    };
    
    /// <summary>
    /// Costruisce un documento valido (Draft è il default)
    /// completo di tutti i campi richiesti
    /// </summary>
    private static Document CreateValidDraftDocument(string id = "1")
    {
        return new Document
        {
            Id = id,
            Number = "N-" + id,
            Date = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Currency = "EUR",
            Customer = new Customer
            {
                Name = "Customer",
                VatNumber = "VatNumber"
            },
            DocumentLines = new List<DocumentLine>
            {
                new() { Description = "A", Quantity = 2, UnitPrice = 10 }
            }
        };
    }
    
    private static Document CreateDocumentStatus(DocumentStatus targetStatus, string id = "1")
    {
        var document = CreateValidDraftDocument(id);

        // Le transizioni di stato sono strette: Draft -> Ready -> Sent -> (Approved|Rejected).
        if (targetStatus == DocumentStatus.Draft)
        {
            return document;
        }

        document.UpdateStatus(DocumentStatus.Ready);
        if (targetStatus == DocumentStatus.Ready)
        {
            return document;
        }

        document.UpdateStatus(DocumentStatus.Sent);
        if (targetStatus == DocumentStatus.Sent)
        {
            return document;
        }

        document.UpdateStatus(targetStatus);
        return document;
    }
    
    #endregion
}
