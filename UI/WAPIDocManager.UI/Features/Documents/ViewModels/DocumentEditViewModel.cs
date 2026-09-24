using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using WAPIDocManager.UI.Features.Documents.Entities;
using WAPIDocManager.UI.Features.Documents.Models;
using WAPIDocManager.UI.Features.Documents.Services;
using WAPIDocManager.UI.Shared.Api;

namespace WAPIDocManager.UI.Features.Documents.ViewModels;

/// <summary>
/// Logica della pagina di modifica documento (MVVM): caricamento del documento, form precompilato e salvataggio.
/// </summary>
/// <remarks>
/// Usato da <c>WAPIDocManager.UI/Features/Documents/Views/Pages/DocumentEdit.razor</c>. La verifica "modificabile"
/// (<c>DocumentRules.CanEdit</c>) e la navigazione dopo il salvataggio restano nel componente; il server rivalida comunque.
/// Registrato Transient.
/// </remarks>
public sealed partial class DocumentEditViewModel : ViewModelBase
{
    private readonly IDocumentService _documentService;

    public DocumentEditViewModel(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    // Stato di proprietà del ViewModel: setter privato + SetProperty (la pagina lo legge e basta)
    private Document? _document;
    private DocumentEditModel _model = new();
    private bool _isLoading = true;
    private bool _isBusy;

    /// <summary>Documento letto dalle API (null se il caricamento è fallito).</summary>
    public Document? Document { get => _document; private set => SetProperty(ref _document, value); }

    /// <summary>Modello del form, precompilato dal documento.</summary>
    public DocumentEditModel Model { get => _model; private set => SetProperty(ref _model, value); }

    public bool IsLoading { get => _isLoading; private set => SetProperty(ref _isLoading, value); }

    public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }

    /// <summary>Errore dell'ultima azione; la pagina lo azzera quando l'utente chiude l'avviso.</summary>
    [ObservableProperty]
    private Exception? _error;

    public async Task LoadAsync(string id)
    {
        IsLoading = true;
        Error = null;

        try
        {
            Document = await _documentService.GetByIdAsync(id);
            Model = (DocumentEditModel)Document;
        }
        catch (Exception ex) when (ex is ApiException or HttpRequestException)
        {
            Document = null;
            Error = ex;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>Salva le modifiche (PUT).</summary>
    /// <returns>true se salvato: la pagina torna al dettaglio.</returns>
    public async Task<bool> SaveAsync(string id)
    {
        IsBusy = true;
        Error = null;

        try
        {
            await _documentService.UpdateAsync(id, Model);
            return true;
        }
        catch (Exception ex) when (ex is ApiException or HttpRequestException)
        {
            Error = ex;
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
