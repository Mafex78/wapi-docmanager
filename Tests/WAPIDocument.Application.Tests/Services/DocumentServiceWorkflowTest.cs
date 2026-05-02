using FluentValidation;
using Moq;
using WAPIDocument.Application.Dto;
using WAPIDocument.Application.Dto.Document;
using WAPIDocument.Application.Services;
using WAPIDocument.Domain.Entities.Documents;
using WAPIDocument.Domain.Repositories;
using Xunit;

namespace WAPIDocument.Application.Tests.Services;

public class DocumentServiceWorkflowTest
{
    private readonly Mock<IDocumentRepository> _repository = new();
    private readonly Mock<IValidator<DocumentCreateRequest>> _documentCreateRequestValidator = new();
    private readonly Mock<IValidator<DocumentUpdateRequest>> _documentUpdateRequestValidator = new();
    private readonly Mock<IValidator<DocumentChangeStatusContext>> _documentChangeStatusValidator = new();
    private readonly Mock<IValidator<DocumentGenerateFromRequest>> _documentGenerateFromRequestValidator = new();

    public DocumentServiceWorkflowTest()
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
    
    [Fact]
    public async Task DocumentService_Workflow_CreateThenDraftToReadyToSent_EndToEnd()
    {
        // ARRANGE: richiesta di creazione valida, completa di tutti i campi richiesti
        //          per poter poi transitare lungo il workflow Draft -> Ready -> Sent.
        var createRequest = new DocumentCreateRequest
        {
            Type = DocumentType.Quote,
            Currency = "EUR",
            Customer = new CustomerDto
            {
                Name = "Acme SRL",
                Email = "billing@acme.test",
                VatNumber = "IT01234567890",
                Address = "Via Roma 1, Milano"
            },
            DocumentLines = new List<DocumentCreateUpdateRequestDocumentLine>
            {
                new() { Description = "Consulenza", Quantity = 3, UnitPrice = 100m },
                new() { Description = "Licenze SW", Quantity = 2, UnitPrice =  50m }
            }
        };

        // Intercetto l'entity che il service inserisce: e' quella costruita dal cast
        // DocumentCreateRequest -> Document e inizializzata via document.Setup(Type),
        // quindi rappresenta lo "stato persistito" che devo poi ri-fornire a GetByIdAsync.
        Document? persisted = null;
        _repository
            .Setup(r => r.InsertAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .Callback<Document, CancellationToken>((d, _) => persisted = d)
            .Returns(Task.CompletedTask);

        _repository
            .Setup(r => r.UpdateAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // ---------------------------------------------------------------------
        // ACT 1: Create
        // ---------------------------------------------------------------------
        var createResponse = await service.CreateAsync(createRequest, CancellationToken.None);

        // ASSERT 1: il service ha prodotto un nuovo aggregato in Draft con tutti gli invarianti.
        Assert.NotNull(createResponse);
        Assert.NotNull(persisted);
        Assert.Equal(createResponse.Id, persisted!.Id);
        Assert.Equal(DocumentStatus.Draft, persisted.Status);
        Assert.Equal(DocumentType.Quote, persisted.Type);
        Assert.False(string.IsNullOrWhiteSpace(persisted.Number));      // Setup() ha generato il Number
        Assert.Equal("EUR", persisted.Currency);
        Assert.Equal("Acme SRL", persisted.Customer!.Name);
        Assert.Equal(2, persisted.DocumentLines!.Count);
        Assert.All(persisted.DocumentLines!, line => Assert.True(line.IsValid()));
        Assert.Equal(400m, persisted.Total); // 3*100 + 2*50

        // Dopo la create il service deve aver chiamato Insert + Save una volta sola.
        _repository.Verify(
            r => r.InsertAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()),
            Times.Once);

        // Da qui in avanti, GetByIdAsync deve restituire la stessa istanza "persistita":
        // stiamo simulando il repository che ri-legge l'aggregato appena salvato.
        _repository
            .Setup(r => r.GetByIdAsync(createResponse.Id!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(persisted);

        // ---------------------------------------------------------------------
        // ACT 2: Draft -> Ready
        // Controlli attesi dentro SetStatus(Draft, Ready):
        //   RequireStatus(Draft) | Currency non vuoto | Customer.Id non vuoto
        //   | Lines non vuote    | ogni riga IsValid()
        // ---------------------------------------------------------------------
        await service.UpdateStatusAsync(createResponse.Id!, DocumentStatus.Ready, CancellationToken.None);

        Assert.Equal(DocumentStatus.Ready, persisted.Status);

        // ---------------------------------------------------------------------
        // ACT 3: Ready -> Sent
        // Stessi controlli di dominio, con RequireStatus(Ready) al posto di Draft.
        // ---------------------------------------------------------------------
        await service.UpdateStatusAsync(createResponse.Id!, DocumentStatus.Sent, CancellationToken.None);

        Assert.Equal(DocumentStatus.Sent, persisted.Status);

        // ---------------------------------------------------------------------
        // ASSERT finale: chiamate complessive al repository e al unit of work.
        //   - InsertAsync: 1 (Create)
        //   - UpdateAsync: 2 (Ready + Sent)
        //   - SaveChangesAsync: 3 (Create + Ready + Sent)
        // ---------------------------------------------------------------------
        _repository.Verify(
            r => r.InsertAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _repository.Verify(
            r => r.UpdateAsync(persisted, It.IsAny<CancellationToken>()),
            Times.Exactly(2));

        // Invarianti del documento intatte dopo l'intero workflow.
        Assert.Equal("EUR", persisted.Currency);
        Assert.Equal("Acme SRL", persisted.Customer!.Name);
        Assert.Equal(2, persisted.DocumentLines!.Count);
        Assert.All(persisted.DocumentLines!, line => Assert.True(line.IsValid()));
        Assert.Equal(400m, persisted.Total);
        Assert.NotNull(persisted.UpdatedAtUtc);
    }

    [Fact]
    public async Task DocumentService_Workflow_CreateThenGenerateFrom_EndToEnd()
    {
        // ARRANGE 
        var createRequest = new DocumentCreateRequest
        {
            Type = DocumentType.Quote,
            Currency = "EUR",
            Customer = new CustomerDto
            {
                Name = "Acme SRL",
                Email = "billing@acme.test",
                VatNumber = "IT01234567890",
                Address = "Via Roma 1, Milano"
            },
            DocumentLines = new List<DocumentCreateUpdateRequestDocumentLine>
            {
                new() { Description = "Consulenza", Quantity = 3, UnitPrice = 100m },
                new() { Description = "Licenze SW", Quantity = 2, UnitPrice = 50m }
            }
        };

        // Intercetto l'entity che il service inserisce: e' quella costruita dal cast
        // DocumentCreateRequest -> Document e inizializzata via document.Setup(Type),
        // quindi rappresenta lo "stato persistito" che devo poi ri-fornire a GetByIdAsync.
        Document? persisted = null;
        _repository
            .Setup(r => r.InsertAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .Callback<Document, CancellationToken>((d, _) => persisted = d)
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // ---------------------------------------------------------------------
        // ACT 1: Create
        // ---------------------------------------------------------------------
        var createResponse = await service.CreateAsync(createRequest, CancellationToken.None);

        // ASSERT 1: il service ha prodotto un nuovo aggregato in Draft con tutti gli invarianti.
        Assert.NotNull(createResponse);
        Assert.NotNull(persisted);
        Assert.Equal(createResponse.Id, persisted!.Id);
        Assert.Equal(DocumentStatus.Draft, persisted.Status);
        Assert.Equal(DocumentType.Quote, persisted.Type);
        Assert.False(string.IsNullOrWhiteSpace(persisted.Number)); // Setup() ha generato il Number
        Assert.Equal("EUR", persisted.Currency);
        Assert.Equal("Acme SRL", persisted.Customer!.Name);
        Assert.Equal(2, persisted.DocumentLines!.Count);
        Assert.All(persisted.DocumentLines!, line => Assert.True(line.IsValid()));
        Assert.Equal(400m, persisted.Total); // 3*100 + 2*50

        // Dopo la create il service deve aver chiamato Insert + Save una volta sola.
        _repository.Verify(
            r => r.InsertAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()),
            Times.Once);

        // Da qui in avanti, GetByIdAsync deve restituire la stessa istanza "persistita":
        // stiamo simulando il repository che ri-legge l'aggregato appena salvato.
        _repository
            .Setup(r => r.GetByIdAsync(createResponse.Id!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(persisted);
        
        Document? newPersisted = null;
        _repository
            .Setup(r => r.InsertAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .Callback<Document, CancellationToken>((d, _) => newPersisted = d)
            .Returns(Task.CompletedTask);
        
        var responseGenerateFrom = await service.GenerateFromAsync(
            createResponse.Id!, 
            new DocumentGenerateFromRequest(DocumentType.Proforma), 
            CancellationToken.None);

        // ASSERT 1: il service ha prodotto un nuovo aggregato in Draft con tutti gli invarianti.
        Assert.NotNull(responseGenerateFrom);
        Assert.NotNull(newPersisted);
        Assert.Equal(responseGenerateFrom.Id, newPersisted!.Id);
        Assert.Equal(DocumentStatus.Draft, newPersisted.Status);
        Assert.Equal(DocumentType.Proforma, newPersisted.Type); // Abbiamo generato una proforma
        Assert.False(string.IsNullOrWhiteSpace(newPersisted.Number)); // Setup() ha generato il Number
        Assert.Equal("EUR", newPersisted.Currency);
        Assert.Equal("Acme SRL", newPersisted.Customer!.Name);
        Assert.Equal(2, newPersisted.DocumentLines!.Count);
        Assert.All(newPersisted.DocumentLines!, line => Assert.True(line.IsValid()));
        Assert.Equal(400m, newPersisted.Total); // 3*100 + 2*50

        // Dopo la GenerateFrom il service deve aver chiamato Insert + Save una volta sola.
        _repository.Verify(
            r => r.InsertAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2)); // Create + GenerateFrom
    }

    [Fact]
    public async Task DocumentService_Workflow_InvalidDocumentStatus_EndToEnd()
    {
        // ARRANGE
        var createRequest = new DocumentCreateRequest
        {
            Type = DocumentType.Quote,
            Currency = "EUR",
            Customer = new CustomerDto
            {
                Name = "Acme SRL",
                Email = "billing@acme.test",
                VatNumber = "IT01234567890",
                Address = "Via Roma 1, Milano"
            },
            DocumentLines = new List<DocumentCreateUpdateRequestDocumentLine>
            {
                new() { Description = "Consulenza", Quantity = 3, UnitPrice = 100m },
                new() { Description = "Licenze SW", Quantity = 2, UnitPrice = 50m }
            }
        };

        // Intercetto l'entity che il service inserisce: e' quella costruita dal cast
        // DocumentCreateRequest -> Document e inizializzata via document.Setup(Type),
        // quindi rappresenta lo "stato persistito" che devo poi ri-fornire a GetByIdAsync.
        Document? persisted = null;
        _repository
            .Setup(r => r.InsertAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .Callback<Document, CancellationToken>((d, _) => persisted = d)
            .Returns(Task.CompletedTask);

        _repository
            .Setup(r => r.UpdateAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // ---------------------------------------------------------------------
        // ACT 1: Create
        // ---------------------------------------------------------------------
        var createResponse = await service.CreateAsync(createRequest, CancellationToken.None);

        // ASSERT 1: il service ha prodotto un nuovo aggregato in Draft con tutti gli invarianti.
        Assert.NotNull(createResponse);
        Assert.NotNull(persisted);
        Assert.Equal(createResponse.Id, persisted!.Id);
        Assert.Equal(DocumentStatus.Draft, persisted.Status);
        Assert.Equal(DocumentType.Quote, persisted.Type);
        Assert.False(string.IsNullOrWhiteSpace(persisted.Number)); // Setup() ha generato il Number
        Assert.Equal("EUR", persisted.Currency);
        Assert.Equal("Acme SRL", persisted.Customer!.Name);
        Assert.Equal(2, persisted.DocumentLines!.Count);
        Assert.All(persisted.DocumentLines!, line => Assert.True(line.IsValid()));
        Assert.Equal(400m, persisted.Total); // 3*100 + 2*50

        // Dopo la create il service deve aver chiamato Insert + Save una volta sola.
        _repository.Verify(
            r => r.InsertAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()),
            Times.Once);

        // Da qui in avanti, GetByIdAsync deve restituire la stessa istanza "persistita":
        // stiamo simulando il repository che ri-legge l'aggregato appena salvato.
        _repository
            .Setup(r => r.GetByIdAsync(createResponse.Id!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(persisted);

        // ---------------------------------------------------------------------
        // ACT 2: Draft -> Sent
        // ERRORE!
        // ---------------------------------------------------------------------
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateStatusAsync(createResponse.Id!, DocumentStatus.Sent, CancellationToken.None));
        
        Assert.Equal(DocumentStatus.Draft, persisted.Status);
        _repository.Verify(r => r.UpdateAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    #region Private methods
    
    private DocumentService CreateService() => new(
        _repository.Object, 
        _documentCreateRequestValidator.Object,
        _documentUpdateRequestValidator.Object,
        _documentChangeStatusValidator.Object,
        _documentGenerateFromRequestValidator.Object);
    
    #endregion
}