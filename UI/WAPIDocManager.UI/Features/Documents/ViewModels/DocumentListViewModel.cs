using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WAPIDocManager.UI.Features.Documents.Entities;
using WAPIDocManager.UI.Features.Documents.Models;
using WAPIDocManager.UI.Features.Documents.Services;
using WAPIDocManager.UI.Shared.Api;

namespace WAPIDocManager.UI.Features.Documents.ViewModels;

/// Logica dell'elenco documenti: ricerca, filtri, paginazione ed eliminazione.
/// La pagina si limita a mostrare questo stato e a inoltrare i comandi. Nel componente restano i ruoli dell'utente,
/// i permessi e i testi tradotti, che sono materia di presentazione.
/// Nasce a ogni visita, così l'elenco riparte pulito; i filtri invece vivono altrove e più a lungo, per sopravvivere
/// all'andata e ritorno verso il dettaglio.
/// Non dipende da Blazor: si limita a notificare i cambiamenti di stato, ed è la classe base del componente a
/// tradurli in un ridisegno. Per questo è verificabile senza renderizzare nulla.
public sealed partial class DocumentListViewModel : ViewModelBase
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

    /// Filtro condiviso con lo stato che sopravvive alla navigazione: il form della pagina lo modifica direttamente.
    public DocumentFilter Filter => _listState.Filter;

    // stato di cui il ViewModel è proprietario: la pagina lo legge e basta
    private PagedResult<Document> _result = new();
    private bool _isLoading = true;
    private bool _isDeleting;

    public PagedResult<Document> Result { get => _result; private set => SetProperty(ref _result, value); }

    public bool IsLoading { get => _isLoading; private set => SetProperty(ref _isLoading, value); }

    public bool IsDeleting { get => _isDeleting; private set => SetProperty(ref _isDeleting, value); }

    // stato che anche la pagina scrive: il setter pubblico è generato dall'attributo

    /// Errore dell'ultima chiamata; la pagina lo azzera quando l'utente chiude l'avviso.
    [ObservableProperty]
    private Exception? _error;

    /// Documento per cui è aperta la richiesta di conferma dell'eliminazione; null significa nessuna richiesta aperta.
    [ObservableProperty]
    private Document? _documentToDelete;

    /// Esegue la ricerca con i filtri correnti.
    /// È un comando e non un semplice metodo perché mentre è in esecuzione si dichiara non eseguibile: è così che i
    /// pulsanti di ricerca, azzeramento e paginazione si disattivano da soli. Prima restavano cliccabili e si
    /// potevano accavallare più ricerche, con il risultato che vinceva l'ultima risposta arrivata e non l'ultima
    /// richiesta fatta.
    /// L'indicatore di caricamento resta comunque necessario: nasce già acceso e copre il primissimo disegno della
    /// pagina, mentre lo stato del comando si attiva solo quando la pagina lo esegue.
    /// Ogni chiamata interna deve passare dal comando: invocare il metodo direttamente salterebbe la guardia.
    [RelayCommand]
    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        IsLoading = true;
        Error = null;

        try
        {
            Result = await _documentService.FindPagedAsync(Filter, cancellationToken);

            // pagina oltre l'ultima, tipicamente dopo un'eliminazione: si torna all'ultima disponibile
            if (Result.Items.Count == 0 && Filter.Page > 1 && Result.TotalPages > 0)
            {
                Filter.Page = Result.TotalPages;
                Result = await _documentService.FindPagedAsync(Filter, cancellationToken);
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

    /// Nuova ricerca, ripartendo dalla prima pagina.
    public Task SearchAsync()
    {
        Filter.Page = 1;
        return LoadCommand.ExecuteAsync(null);
    }

    /// Azzera i filtri sostituendo l'istanza condivisa, e ricarica.
    public Task ResetAsync()
    {
        _listState.Filter = new DocumentFilter();
        return LoadCommand.ExecuteAsync(null);
    }

    public Task GoToPageAsync(int page)
    {
        Filter.Page = page;
        return LoadCommand.ExecuteAsync(null);
    }

    /// Elimina il documento per cui è stata chiesta conferma, e ricarica la pagina corrente.
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
            await LoadCommand.ExecuteAsync(null);
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

    /// Aggiunge o toglie la tipologia dal filtro. Non fa partire la ricerca: resta all'utente decidere quando,
    /// così può comporre più criteri prima di cercare.
    public void ToggleType(DocumentType type)
    {
        Toggle(Filter.Types, type);
    }

    /// Aggiunge o toglie lo stato dal filtro, con lo stesso criterio.
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
