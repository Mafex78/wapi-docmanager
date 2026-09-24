using System.Net;
using Moq;
using WAPIDocManager.UI.Features.Documents.Entities;
using WAPIDocManager.UI.Features.Documents.Models;
using WAPIDocManager.UI.Features.Documents.Services;
using WAPIDocManager.UI.Features.Documents.ViewModels;
using WAPIDocManager.UI.Features.Documents;
using WAPIDocManager.UI.Shared.Api;

namespace WAPIDocManager.UI.Tests.Features.Documents;

/// <summary>
/// Logica della pagina lista documenti: ricerca, filtri condivisi, paginazione, eliminazione ed errori.
/// Testabile senza Blazor perché il ViewModel dipende solo da IDocumentService e DocumentListState.
/// </summary>
public class DocumentListViewModelTests
{
    [Fact]
    public async Task SearchAsync_Restarts_From_First_Page()
    {
        (DocumentListViewModel viewModel, Mock<IDocumentService> service, _) = Create(EmptyPage());
        viewModel.Filter.Page = 4;

        await viewModel.SearchAsync();

        Assert.Equal(1, viewModel.Filter.Page);
        service.Verify(s => s.FindPagedAsync(It.IsAny<DocumentFilter>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResetAsync_Replaces_The_Filter_In_The_Shared_State()
    {
        (DocumentListViewModel viewModel, _, DocumentListState state) = Create(EmptyPage());
        viewModel.Filter.CustomerName = "ACME";
        DocumentFilter previousFilter = state.Filter;

        await viewModel.ResetAsync();

        Assert.NotSame(previousFilter, state.Filter);
        Assert.Same(state.Filter, viewModel.Filter);
        Assert.Null(viewModel.Filter.CustomerName);
    }

    [Fact]
    public async Task LoadAsync_Beyond_Last_Page_Falls_Back_To_The_Last_Available_Page()
    {
        var service = new Mock<IDocumentService>();
        service.SetupSequence(s => s.FindPagedAsync(It.IsAny<DocumentFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Document> { TotalPages = 2, TotalItems = 21 })
            .ReturnsAsync(new PagedResult<Document>
            {
                CurrentPage = 2,
                TotalPages = 2,
                TotalItems = 21,
                Items = new[] { new Document { Id = "abc" } }
            });
        var viewModel = new DocumentListViewModel(service.Object, new DocumentListState());
        viewModel.Filter.Page = 3;

        await viewModel.LoadCommand.ExecuteAsync(null);

        Assert.Equal(2, viewModel.Filter.Page);
        Assert.Single(viewModel.Result.Items);
        service.Verify(s => s.FindPagedAsync(It.IsAny<DocumentFilter>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Load_Command_Is_Not_Executable_While_Running()
    {
        var pending = new TaskCompletionSource<PagedResult<Document>>();
        var service = new Mock<IDocumentService>();
        service.Setup(s => s.FindPagedAsync(It.IsAny<DocumentFilter>(), It.IsAny<CancellationToken>()))
            .Returns(pending.Task);
        var viewModel = new DocumentListViewModel(service.Object, new DocumentListState());

        Task loading = viewModel.LoadCommand.ExecuteAsync(null);

        // è così che il markup disabilita Cerca, Azzera e il Pager: prima restavano cliccabili e due ricerche
        // potevano sovrapporsi, con l'esito dell'ultima risposta arrivata
        Assert.True(viewModel.LoadCommand.IsRunning);
        Assert.False(viewModel.LoadCommand.CanExecute(null));

        pending.SetResult(new PagedResult<Document>());
        await loading;

        Assert.False(viewModel.LoadCommand.IsRunning);
        Assert.True(viewModel.LoadCommand.CanExecute(null));
    }

    [Fact]
    public async Task SearchAsync_Goes_Through_The_Command_And_Resets_The_Page()
    {
        var pending = new TaskCompletionSource<PagedResult<Document>>();
        var service = new Mock<IDocumentService>();
        service.Setup(s => s.FindPagedAsync(It.IsAny<DocumentFilter>(), It.IsAny<CancellationToken>()))
            .Returns(pending.Task);
        var viewModel = new DocumentListViewModel(service.Object, new DocumentListState());
        viewModel.Filter.Page = 4;

        Task searching = viewModel.SearchAsync();

        // passa dal comando: chiamare il metodo direttamente salterebbe IsRunning e la guardia non scatterebbe
        Assert.True(viewModel.LoadCommand.IsRunning);
        Assert.Equal(1, viewModel.Filter.Page);

        pending.SetResult(new PagedResult<Document>());
        await searching;
    }

    [Fact]
    public async Task LoadAsync_Api_Error_Sets_Error_And_Clears_The_Result()
    {
        var service = new Mock<IDocumentService>();
        service.Setup(s => s.FindPagedAsync(It.IsAny<DocumentFilter>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiException(HttpStatusCode.BadRequest, "PageSize must be less than or equal to 20."));
        var viewModel = new DocumentListViewModel(service.Object, new DocumentListState());

        await viewModel.LoadCommand.ExecuteAsync(null);

        Assert.IsType<ApiException>(viewModel.Error);
        Assert.Empty(viewModel.Result.Items);
        Assert.False(viewModel.IsLoading);
    }

    [Fact]
    public async Task DeleteAsync_Deletes_The_Selected_Document_And_Reloads()
    {
        (DocumentListViewModel viewModel, Mock<IDocumentService> service, _) = Create(EmptyPage());
        viewModel.DocumentToDelete = new Document { Id = "abc" };

        await viewModel.DeleteAsync();

        service.Verify(s => s.DeleteAsync("abc", It.IsAny<CancellationToken>()), Times.Once);
        service.Verify(s => s.FindPagedAsync(It.IsAny<DocumentFilter>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.Null(viewModel.DocumentToDelete);
        Assert.False(viewModel.IsDeleting);
    }

    [Fact]
    public async Task DeleteAsync_Api_Error_Closes_The_Dialog_And_Sets_Error()
    {
        (DocumentListViewModel viewModel, Mock<IDocumentService> service, _) = Create(EmptyPage());
        service.Setup(s => s.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiException(HttpStatusCode.BadRequest, "Cannot delete a document in status Sent."));
        viewModel.DocumentToDelete = new Document { Id = "abc" };

        await viewModel.DeleteAsync();

        Assert.Null(viewModel.DocumentToDelete);
        Assert.IsType<ApiException>(viewModel.Error);
    }

    [Fact]
    public void ToggleType_And_ToggleStatus_Add_Then_Remove_The_Value()
    {
        (DocumentListViewModel viewModel, _, _) = Create(EmptyPage());

        viewModel.ToggleType(DocumentType.Proforma);
        viewModel.ToggleStatus(DocumentStatus.Sent);

        Assert.Equal(new[] { DocumentType.Proforma }, viewModel.Filter.Types);
        Assert.Equal(new[] { DocumentStatus.Sent }, viewModel.Filter.Statuses);

        viewModel.ToggleType(DocumentType.Proforma);
        viewModel.ToggleStatus(DocumentStatus.Sent);

        Assert.Empty(viewModel.Filter.Types);
        Assert.Empty(viewModel.Filter.Statuses);
    }

    private static PagedResult<Document> EmptyPage()
    {
        return new PagedResult<Document> { CurrentPage = 1, PageSize = DocumentFilter.MaxPageSize };
    }

    private static (DocumentListViewModel ViewModel, Mock<IDocumentService> Service, DocumentListState State) Create(PagedResult<Document> page)
    {
        var service = new Mock<IDocumentService>();
        service.Setup(s => s.FindPagedAsync(It.IsAny<DocumentFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        var state = new DocumentListState();

        return (new DocumentListViewModel(service.Object, state), service, state);
    }
}
