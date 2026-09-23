using CommunityToolkit.Mvvm.ComponentModel;
using WAPIDocManager.UI.Features.Documents.Entities;
using WAPIDocManager.UI.Features.Documents.Models;
using WAPIDocManager.UI.Features.Documents.Services;
using WAPIDocManager.UI.Features.Documents.Views.Pages;
using WAPIDocManager.UI.Features.Documents;
using WAPIDocManager.UI.Shared.Api;

namespace WAPIDocManager.UI.Features.Documents.ViewModels;

/// <summary>
/// Logica della pagina lista documenti (MVVM): ricerca, filtri, paginazione ed eliminazione.
/// </summary>
/// <remarks>
/// <para>
/// Usato da <c>WAPIDocManager.UI/Features/Documents/Views/Pages/DocumentList.razor</c>, che si limita a mostrare lo stato
/// e a inoltrare i comandi. Nel componente restano ruoli utente, permessi (<c>DocumentPermissions</c>) e testi localizzati.
/// </para>
/// <para>
/// Registrato Transient: ogni visita alla pagina parte da uno stato pulito; i filtri vivono invece in
/// <see cref="DocumentListState"/> (Scoped), così sopravvivono alla navigazione verso il dettaglio e ritorno.
/// </para>
/// <para>
/// Nessuna dipendenza da Blazor: deriva da ObservableObject (CommunityToolkit.Mvvm) e notifica i cambiamenti di
/// proprietà; è <c>Shared/Mvvm/MvvmComponentBase</c> a tradurli in un ridisegno del componente.
/// </para>
/// </remarks>
public sealed partial class DocumentListViewModel : ObservableObject
{
    private readonly IDocumentService _documentService;
    private readonly DocumentListState _listState;

    public DocumentListViewModel(
        IDocumentService documentService,
        DocumentListState listState)
    {
        _documentService = documentService;
        _listState = listState;
    }

    /// <summary>Filtro condiviso con <see cref="DocumentListState"/>: il form della pagina lo modifica direttamente.</summary>
    public DocumentFilter Filter => _listState.Filter;

    // Stato di proprietà del ViewModel: setter privato + SetProperty (la pagina lo legge e basta)
    private PagedResult<Document> _result = new();
    private bool _isLoading = true;
    private bool _isDeleting;

    public PagedResult<Document> Result { get => _result; private set => SetProperty(ref _result, value); }

    public bool IsLoading { get => _isLoading; private set => SetProperty(ref _isLoading, value); }

    public bool IsDeleting { get => _isDeleting; private set => SetProperty(ref _isDeleting, value); }

    // Stato scritto dalla pagina: setter pubblico generato da [ObservableProperty]

    /// <summary>Errore dell'ultima chiamata; la pagina lo azzera quando l'utente chiude l'avviso.</summary>
    [ObservableProperty]
    private Exception? _error;

    /// <summary>Documento per cui è aperta la modale di conferma eliminazione (null = modale chiusa).</summary>
    [ObservableProperty]
    private Document? _documentToDelete;

    /// <summary>Esegue la ricerca con i filtri correnti.</summary>
    public async Task LoadAsync()
    {
        IsLoading = true;
        Error = null;

        try
        {
            Result = await _documentService.FindPagedAsync(Filter);

            // pagina oltre l'ultima (es. dopo un'eliminazione): torna all'ultima disponibile
            if (Result.Items.Count == 0 && Filter.Page > 1 && Result.TotalPages > 0)
            {
                Filter.Page = Result.TotalPages;
                Result = await _documentService.FindPagedAsync(Filter);
            }
        }
        catch (Exception ex) when (ex is ApiException or HttpRequestException)
        {
            Error = ex;
            Result = new PagedResult<Document>();
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>Nuova ricerca dalla prima pagina.</summary>
    public Task SearchAsync()
    {
        Filter.Page = 1;
        return LoadAsync();
    }

    /// <summary>Azzera i filtri sostituendo l'istanza condivisa e ricarica.</summary>
    public Task ResetAsync()
    {
        _listState.Filter = new DocumentFilter();
        return LoadAsync();
    }

    public Task GoToPageAsync(int page)
    {
        Filter.Page = page;
        return LoadAsync();
    }

    /// <summary>Elimina il documento in <see cref="DocumentToDelete"/> e ricarica la pagina corrente.</summary>
    public async Task DeleteAsync()
    {
        if (DocumentToDelete is null)
        {
            return;
        }

        IsDeleting = true;

        try
        {
            await _documentService.DeleteAsync(DocumentToDelete.Id);
            DocumentToDelete = null;
            await LoadAsync();
        }
        catch (Exception ex) when (ex is ApiException or HttpRequestException)
        {
            DocumentToDelete = null;
            Error = ex;
        }
        finally
        {
            IsDeleting = false;
        }
    }

    /// <summary>Aggiunge o rimuove la tipologia dal filtro (chip della pagina). La ricerca resta a carico dell'utente.</summary>
    public void ToggleType(DocumentType type)
    {
        Toggle(Filter.Types, type);
    }

    /// <summary>Aggiunge o rimuove lo stato dal filtro (chip della pagina).</summary>
    public void ToggleStatus(DocumentStatus status)
    {
        Toggle(Filter.Statuses, status);
    }

    private static void Toggle<T>(List<T> values, T value)
    {
        if (!values.Remove(value))
        {
            values.Add(value);
        }
    }
}
