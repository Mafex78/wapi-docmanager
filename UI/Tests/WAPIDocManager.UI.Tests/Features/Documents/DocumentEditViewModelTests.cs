using System.Net;
using Moq;
using WAPIDocManager.UI.Features.Documents.Entities;
using WAPIDocManager.UI.Features.Documents.Models;
using WAPIDocManager.UI.Features.Documents.Services;
using WAPIDocManager.UI.Features.Documents.ViewModels;
using WAPIDocManager.UI.Shared.Api;

namespace WAPIDocManager.UI.Tests.Features.Documents;

/// <summary>
/// Logica della pagina di modifica documento: caricamento con form precompilato e salvataggio.
/// La verifica "modificabile" e la navigazione restano nella pagina.
/// </summary>
public class DocumentEditViewModelTests
{
    [Fact]
    public async Task LoadAsync_Fills_The_Form_From_The_Document()
    {
        var document = new Document
        {
            Id = "abc",
            Type = DocumentType.Proforma,
            Status = DocumentStatus.Ready,
            Date = new DateTime(2026, 9, 18, 0, 0, 0, DateTimeKind.Utc),
            Customer = new Customer { Name = "ACME", VatNumber = "IT123" },
            Lines = new[] { new DocumentLine { Description = "Line", Quantity = 2, UnitPrice = 5, Total = 10 } }
        };
        var service = new Mock<IDocumentService>();
        service.Setup(s => s.GetByIdAsync("abc", It.IsAny<CancellationToken>())).ReturnsAsync(document);
        var viewModel = new DocumentEditViewModel(service.Object);

        await viewModel.LoadAsync("abc");

        Assert.Same(document, viewModel.Document);
        Assert.False(viewModel.IsLoading);
        Assert.Equal(DocumentType.Proforma, viewModel.Model.Type);
        Assert.Equal("ACME", viewModel.Model.Customer.Name);
        Assert.Single(viewModel.Model.Lines);
    }

    [Fact]
    public async Task LoadAsync_Not_Found_Leaves_No_Document_And_Sets_Error()
    {
        var service = new Mock<IDocumentService>();
        service.Setup(s => s.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiException(HttpStatusCode.NotFound, null));
        var viewModel = new DocumentEditViewModel(service.Object);

        await viewModel.LoadAsync("abc");

        Assert.Null(viewModel.Document);
        Assert.IsType<ApiException>(viewModel.Error);
    }

    [Fact]
    public async Task SaveAsync_Returns_True_And_Sends_The_Form()
    {
        var service = new Mock<IDocumentService>();
        var viewModel = new DocumentEditViewModel(service.Object);

        bool saved = await viewModel.SaveAsync("abc");

        Assert.True(saved);
        Assert.Null(viewModel.Error);
        Assert.False(viewModel.IsBusy);
        service.Verify(s => s.UpdateAsync("abc", It.IsAny<DocumentEditModel>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SaveAsync_Api_Error_Returns_False_And_Sets_Error()
    {
        var service = new Mock<IDocumentService>();
        service.Setup(s => s.UpdateAsync(It.IsAny<string>(), It.IsAny<DocumentEditModel>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiException(HttpStatusCode.BadRequest, "Cannot edit a document in status Sent."));
        var viewModel = new DocumentEditViewModel(service.Object);

        bool saved = await viewModel.SaveAsync("abc");

        Assert.False(saved);
        Assert.IsType<ApiException>(viewModel.Error);
    }
}
