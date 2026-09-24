using System.Net;
using Moq;
using WAPIDocManager.UI.Features.Documents.Entities;
using WAPIDocManager.UI.Features.Documents.Models;
using WAPIDocManager.UI.Features.Documents.Services;
using WAPIDocManager.UI.Features.Documents.ViewModels;
using WAPIDocManager.UI.Shared.Api;

namespace WAPIDocManager.UI.Tests.Features.Documents;

/// <summary>
/// Logica della pagina di dettaglio documento: caricamento (compresi i collegati), avanzamento di stato,
/// generazione, collegamento ed eliminazione. Navigazione e testi restano nella pagina, quindi qui si
/// verificano esiti (bool, documento generato) e notifiche.
/// </summary>
public class DocumentDetailViewModelTests
{
    [Fact]
    public async Task LoadAsync_Loads_The_Document_And_The_Readable_Linked_Documents()
    {
        var service = new Mock<IDocumentService>();
        service.Setup(s => s.GetByIdAsync("abc", It.IsAny<CancellationToken>()))
            .ReturnsAsync(DocumentWithLinks("abc", "ok", "gone"));
        service.Setup(s => s.GetByIdAsync("ok", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Document { Id = "ok", Number = "N-OK" });
        service.Setup(s => s.GetByIdAsync("gone", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiException(HttpStatusCode.NotFound, null));
        var viewModel = new DocumentDetailViewModel(service.Object);
        var notified = new List<string?>();
        viewModel.PropertyChanged += (_, e) => notified.Add(e.PropertyName);

        await viewModel.LoadAsync("abc");

        Assert.NotNull(viewModel.Document);
        Assert.False(viewModel.IsLoading);
        Assert.False(viewModel.IsLoadingLinks);
        Assert.Equal("N-OK", viewModel.TryGetLinkedDocument("ok")?.Number);
        Assert.Null(viewModel.TryGetLinkedDocument("gone"));
        // i documenti collegati si caricano dopo la pagina: le due notifiche di IsLoadingLinks (true e poi false)
        // sono ciò che fa comparire prima lo spinner e poi l'elenco, al posto del vecchio evento StateChanged
        Assert.Equal(2, notified.Count(name => name == nameof(DocumentDetailViewModel.IsLoadingLinks)));
    }

    [Fact]
    public void ObservableProperty_Setter_Notifies_The_Page()
    {
        (DocumentDetailViewModel viewModel, Mock<IDocumentService> _) = Create();
        var notified = new List<string?>();
        viewModel.PropertyChanged += (_, e) => notified.Add(e.PropertyName);

        viewModel.ShowAttach = true;
        viewModel.Error = new InvalidOperationException();

        // proprietà scritte dal markup: il setter generato da [ObservableProperty] deve notificare,
        // altrimenti la pagina non si ridisegna quando lo stato cambia fuori da un gestore di evento
        Assert.Equal(
            new[] { nameof(DocumentDetailViewModel.ShowAttach), nameof(DocumentDetailViewModel.Error) },
            notified);
    }

    [Fact]
    public async Task LoadAsync_Does_Not_Reload_The_Same_Document()
    {
        (DocumentDetailViewModel viewModel, Mock<IDocumentService> service) = Create();

        await viewModel.LoadAsync("abc");
        await viewModel.LoadAsync("abc");

        service.Verify(s => s.GetByIdAsync("abc", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoadAsync_Api_Error_Leaves_No_Document_And_Sets_Error()
    {
        var service = new Mock<IDocumentService>();
        service.Setup(s => s.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiException(HttpStatusCode.NotFound, null));
        var viewModel = new DocumentDetailViewModel(service.Object);

        await viewModel.LoadAsync("abc");

        Assert.Null(viewModel.Document);
        Assert.IsType<ApiException>(viewModel.Error);
    }

    [Fact]
    public async Task ChangeStatusAsync_Updates_The_Document_And_Notifies_The_New_Status()
    {
        (DocumentDetailViewModel viewModel, Mock<IDocumentService> service) = Create();
        service.Setup(s => s.UpdateStatusAsync("abc", DocumentStatus.Ready, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Document { Id = "abc", Status = DocumentStatus.Ready });
        await viewModel.LoadAsync("abc");

        await viewModel.ChangeStatusAsync(DocumentStatus.Ready);

        Assert.Equal(DocumentStatus.Ready, viewModel.Document!.Status);
        Assert.Equal(DocumentDetailNotification.StatusUpdated, viewModel.Notification);
        Assert.Equal(DocumentStatus.Ready, viewModel.NotificationStatus);
        Assert.False(viewModel.IsBusy);

        viewModel.ClearNotification();

        Assert.Equal(DocumentDetailNotification.None, viewModel.Notification);
        Assert.Null(viewModel.NotificationStatus);
    }

    [Fact]
    public async Task DeleteAsync_Returns_True_And_Closes_The_Dialog()
    {
        (DocumentDetailViewModel viewModel, Mock<IDocumentService> service) = Create();
        await viewModel.LoadAsync("abc");
        viewModel.ConfirmDelete = true;

        bool deleted = await viewModel.DeleteAsync();

        Assert.True(deleted);
        Assert.False(viewModel.ConfirmDelete);
        service.Verify(s => s.DeleteAsync("abc", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_Api_Error_Returns_False_And_Sets_Error()
    {
        (DocumentDetailViewModel viewModel, Mock<IDocumentService> service) = Create();
        service.Setup(s => s.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiException(HttpStatusCode.BadRequest, "Cannot delete a document in status Sent."));
        await viewModel.LoadAsync("abc");

        bool deleted = await viewModel.DeleteAsync();

        Assert.False(deleted);
        Assert.IsType<ApiException>(viewModel.Error);
    }

    [Fact]
    public async Task GenerateAsync_Returns_The_Generated_Document_And_Closes_The_Dialog()
    {
        (DocumentDetailViewModel viewModel, Mock<IDocumentService> service) = Create();
        service.Setup(s => s.GenerateFromAsync("abc", DocumentType.Proforma, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Document { Id = "generated", Type = DocumentType.Proforma });
        await viewModel.LoadAsync("abc");
        viewModel.ShowGenerate = true;

        // tipo proposto per un preventivo: proforma
        Assert.Equal(DocumentType.Proforma, viewModel.GenerateType);

        Document? generated = await viewModel.GenerateAsync();

        Assert.Equal("generated", generated?.Id);
        Assert.False(viewModel.ShowGenerate);
    }

    [Fact]
    public async Task GenerateAsync_Api_Error_Returns_Null_And_Sets_Error()
    {
        (DocumentDetailViewModel viewModel, Mock<IDocumentService> service) = Create();
        service.Setup(s => s.GenerateFromAsync(It.IsAny<string>(), It.IsAny<DocumentType>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiException(HttpStatusCode.BadRequest, "Cannot generate."));
        await viewModel.LoadAsync("abc");

        Document? generated = await viewModel.GenerateAsync();

        Assert.Null(generated);
        Assert.IsType<ApiException>(viewModel.Error);
    }

    [Fact]
    public async Task AttachCandidates_Exclude_The_Document_Itself_And_The_Already_Linked_Ones()
    {
        var service = new Mock<IDocumentService>();
        service.Setup(s => s.GetByIdAsync("abc", It.IsAny<CancellationToken>()))
            .ReturnsAsync(DocumentWithLinks("abc", "linked"));
        service.Setup(s => s.GetByIdAsync("linked", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Document { Id = "linked" });
        service.Setup(s => s.FindPagedAsync(It.IsAny<DocumentFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Document>
            {
                Items = new[]
                {
                    new Document { Id = "abc" },
                    new Document { Id = "linked" },
                    new Document { Id = "free" }
                }
            });
        var viewModel = new DocumentDetailViewModel(service.Object);
        await viewModel.LoadAsync("abc");

        await viewModel.OpenAttachAsync();

        Assert.True(viewModel.ShowAttach);
        Assert.Equal(DocumentDetailViewModel.AttachPageSize, viewModel.AttachFilter.PageSize);
        Assert.Equal(new[] { "free" }, viewModel.GetAttachCandidates().Select(candidate => candidate.Id));
    }

    [Fact]
    public async Task AttachAsync_Updates_The_Links_And_Notifies()
    {
        (DocumentDetailViewModel viewModel, Mock<IDocumentService> service) = Create();
        service.Setup(s => s.AttachAsync("abc", "other", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { new DocumentLink { TargetDocumentId = "other" } });
        service.Setup(s => s.GetByIdAsync("other", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Document { Id = "other", Number = "N-OTHER" });
        await viewModel.LoadAsync("abc");

        await viewModel.AttachAsync(new Document { Id = "other" });

        Assert.Equal(new[] { "other" }, viewModel.Document!.LinkedDocuments.Select(link => link.TargetDocumentId));
        Assert.Equal("N-OTHER", viewModel.TryGetLinkedDocument("other")?.Number);
        Assert.Equal(DocumentDetailNotification.Attached, viewModel.Notification);
        Assert.False(viewModel.ShowAttach);
    }

    [Fact]
    public async Task AttachAsync_Api_Error_Keeps_The_Dialog_Open_With_The_Error()
    {
        (DocumentDetailViewModel viewModel, Mock<IDocumentService> service) = Create();
        service.Setup(s => s.AttachAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiException(HttpStatusCode.BadRequest, "Document already attached."));
        await viewModel.LoadAsync("abc");
        await viewModel.OpenAttachAsync();

        await viewModel.AttachAsync(new Document { Id = "other" });

        Assert.True(viewModel.ShowAttach);
        Assert.IsType<ApiException>(viewModel.AttachError);
        Assert.Empty(viewModel.Document!.LinkedDocuments);
    }

    private static Document DocumentWithLinks(string id, params string[] linkedIds)
    {
        return new Document
        {
            Id = id,
            Number = "N-1",
            Type = DocumentType.Quote,
            Status = DocumentStatus.Draft,
            LinkedDocuments = linkedIds
                .Select(linkedId => new DocumentLink { TargetDocumentId = linkedId })
                .ToList()
        };
    }

    [Fact]
    public async Task SearchAttach_Command_Is_Not_Executable_While_Running()
    {
        var pending = new TaskCompletionSource<PagedResult<Document>>();
        var service = new Mock<IDocumentService>();
        service.Setup(s => s.GetByIdAsync("abc", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Document { Id = "abc" });
        service.Setup(s => s.FindPagedAsync(It.IsAny<DocumentFilter>(), It.IsAny<CancellationToken>()))
            .Returns(pending.Task);
        var viewModel = new DocumentDetailViewModel(service.Object);
        await viewModel.LoadAsync("abc");

        Task opening = viewModel.OpenAttachAsync();

        // è questo che permette al markup di disabilitare il pulsante Cerca e di mostrare lo spinner:
        // prima della conversione a comando il pulsante restava attivo e due ricerche potevano sovrapporsi
        Assert.True(viewModel.SearchAttachCommand.IsRunning);
        Assert.False(viewModel.SearchAttachCommand.CanExecute(null));

        pending.SetResult(new PagedResult<Document>());
        await opening;

        Assert.False(viewModel.SearchAttachCommand.IsRunning);
        Assert.True(viewModel.SearchAttachCommand.CanExecute(null));
    }

    private static (DocumentDetailViewModel ViewModel, Mock<IDocumentService> Service) Create()
    {
        var service = new Mock<IDocumentService>();
        service.Setup(s => s.GetByIdAsync("abc", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Document { Id = "abc", Number = "N-1", Type = DocumentType.Quote });
        service.Setup(s => s.FindPagedAsync(It.IsAny<DocumentFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Document>());

        return (new DocumentDetailViewModel(service.Object), service);
    }
}
