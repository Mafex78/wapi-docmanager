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
public sealed class DocumentEditViewModel
{
    private readonly IDocumentService _documentService;

    public DocumentEditViewModel(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    /// <summary>Documento letto dalle API (null se il caricamento è fallito).</summary>
    public Document? Document { get; private set; }

    /// <summary>Modello del form, precompilato dal documento.</summary>
    public DocumentEditModel Model { get; private set; } = new();

    public Exception? Error { get; set; }

    public bool IsLoading { get; private set; } = true;

    public bool IsBusy { get; private set; }

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
