using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using WAPIDocManager.UI.Features.Documents.Entities;
using WAPIDocManager.UI.Features.Documents.Models;
using WAPIDocManager.UI.Features.Documents.Services;
using WAPIDocManager.UI.Shared.Api;

namespace WAPIDocManager.UI.Features.Documents.ViewModels;

/// Logica della modifica di un documento: lettura, form precompilato e salvataggio.
/// Il controllo che il documento sia ancora modificabile e la navigazione dopo il salvataggio restano nella pagina.
/// In ogni caso il server rivalida: qui si evita solo all'utente un tentativo inutile.
public sealed partial class DocumentEditViewModel : ViewModelBase
{
    private readonly IDocumentService _documentService;

    public DocumentEditViewModel(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    // stato di cui il ViewModel è proprietario: la pagina lo legge e basta
    private Document? _document;
    private DocumentEditModel _model = new();
    private bool _isLoading = true;
    private bool _isBusy;

    /// Documento letto dalle API; null se la lettura è fallita.
    public Document? Document { get => _document; private set => SetProperty(ref _document, value); }

    /// Modello del form, precompilato a partire dal documento.
    public DocumentEditModel Model { get => _model; private set => SetProperty(ref _model, value); }

    public bool IsLoading { get => _isLoading; private set => SetProperty(ref _isLoading, value); }

    public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }

    /// Errore dell'ultima azione; la pagina lo azzera quando l'utente chiude l'avviso.
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

    /// Salva le modifiche. Restituisce l'esito invece di navigare: dove andare dopo lo decide la pagina.
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
