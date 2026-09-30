using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WAPIDocManager.UI.Features.Documents.Entities;
using WAPIDocManager.UI.Features.Documents.Models;
using WAPIDocManager.UI.Features.Documents.Services;
using WAPIDocManager.UI.Shared.Api;

namespace WAPIDocManager.UI.Features.Documents.ViewModels;

/// Logica del dettaglio del documento: lettura, avanzamento di stato, generazione, collegamento, eliminazione e
/// lettura dei documenti collegati.
/// Restano nella pagina i ruoli e i permessi, i testi tradotti e la navigazione, che dipende dall'esito delle azioni
/// che restituiscono un valore.
/// Passando da un documento all'altro il componente viene riusato: non nasce un'istanza nuova, e per questo la
/// rilettura è governata da una guardia sull'identificativo già caricato.
public sealed partial class DocumentDetailViewModel : ViewModelBase
{
    /// Risultati per pagina nella ricerca dei documenti da collegare.
    public const int AttachPageSize = 10;

    private readonly IDocumentService _documentService;
    private readonly Dictionary<string, Document> _linkedDocuments = new();

    private PagedResult<Document> _attachResult = new();
    private string? _loadedId;

    public DocumentDetailViewModel(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    // stato di cui il ViewModel è proprietario: la pagina lo legge e basta.
    // Scritto a mano e non generato dall'attributo, perché il generatore crea sempre un setter pubblico
    private Document? _document;
    private DocumentDetailNotification _notification;
    private DocumentStatus? _notificationStatus;
    private bool _isLoading = true;
    private bool _isLoadingLinks;
    private bool _isBusy;
    private DocumentFilter _attachFilter = new() { PageSize = AttachPageSize };

    public Document? Document { get => _document; private set => SetProperty(ref _document, value); }

    public DocumentDetailNotification Notification { get => _notification; private set => SetProperty(ref _notification, value); }

    /// Stato raggiunto, valorizzato solo quando la notifica riguarda un cambio di stato.
    public DocumentStatus? NotificationStatus { get => _notificationStatus; private set => SetProperty(ref _notificationStatus, value); }

    public bool IsLoading { get => _isLoading; private set => SetProperty(ref _isLoading, value); }

    /// I documenti collegati si leggono dopo il resto della pagina: è la notifica di questo valore a far comparire
    /// prima l'indicatore di caricamento e poi l'elenco.
    public bool IsLoadingLinks { get => _isLoadingLinks; private set => SetProperty(ref _isLoadingLinks, value); }

    /// Azione in corso: la pagina disattiva i pulsanti.
    public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }

    public DocumentFilter AttachFilter { get => _attachFilter; private set => SetProperty(ref _attachFilter, value); }

    // stato che la pagina scrive direttamente dal markup: il setter pubblico è generato dall'attributo

    /// Errore dell'ultima azione; la pagina lo azzera quando l'utente chiude l'avviso.
    [ObservableProperty]
    private Exception? _error;

    [ObservableProperty]
    private bool _confirmDelete;

    [ObservableProperty]
    private bool _showGenerate;

    /// Tipologia proposta per la generazione, che l'utente può cambiare prima di confermare.
    [ObservableProperty]
    private DocumentType _generateType;

    [ObservableProperty]
    private bool _showAttach;

    [ObservableProperty]
    private Exception? _attachError;

    /// Candidati al collegamento: esclude il documento corrente e quelli già collegati.
    /// È un metodo e non una proprietà perché a ogni chiamata filtra i risultati e costruisce una lista nuova: il
    /// costo deve essere visibile a chi lo usa dal markup, dove una proprietà sembrerebbe gratuita.
    public IReadOnlyList<Document> GetAttachCandidates()
    {
        return _attachResult.Items
            .Where(candidate => candidate.Id != Document?.Id &&
                                Document?.LinkedDocuments.Any(link => link.TargetDocumentId == candidate.Id) != true)
            .ToList();
    }

    /// Documento collegato già letto, oppure null se non è stato possibile leggerlo, per esempio perché nel
    /// frattempo è stato eliminato.
    public Document? TryGetLinkedDocument(string id)
    {
        return _linkedDocuments.GetValueOrDefault(id);
    }

    /// Righe dei documenti collegati: per ciascun collegamento, il documento già letto oppure null.
    /// Il markup scorre queste coppie invece di cercare il documento dentro il ciclo: il recupero resta qui e la
    /// pagina si limita a mostrare.
    public IReadOnlyList<(DocumentLink Link, Document? Document)> GetLinkedRows()
    {
        if (Document is null)
        {
            return [];
        }

        return Document.LinkedDocuments
            .Select(link => (link, TryGetLinkedDocument(link.TargetDocumentId)))
            .ToList();
    }

    public void ClearNotification()
    {
        Notification = DocumentDetailNotification.None;
        NotificationStatus = null;
    }

    /// Legge il documento e i suoi collegati. Rilegge solo se l'identificativo è cambiato, perché la pagina viene
    /// riusata passando da un documento all'altro.
    public async Task LoadAsync(string id)
    {
        if (_loadedId == id)
        {
            return;
        }

        _loadedId = id;
        Error = null;
        ClearNotification();
        ConfirmDelete = false;
        ShowGenerate = false;
        ShowAttach = false;
        IsLoading = true;

        try
        {
            SetDocument(await _documentService.GetByIdAsync(id));
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

        await LoadLinkedDocumentsAsync();
    }

    public async Task ChangeStatusAsync(DocumentStatus newStatus)
    {
        await RunAsync(async () =>
        {
            SetDocument(await _documentService.UpdateStatusAsync(Document!.Id, newStatus));
            Notification = DocumentDetailNotification.StatusUpdated;
            NotificationStatus = newStatus;
        });
    }

    /// Elimina il documento corrente. Restituisce l'esito: dove andare dopo lo decide la pagina.
    public async Task<bool> DeleteAsync()
    {
        bool deleted = await RunAsync(async () => await _documentService.DeleteAsync(Document!.Id));

        ConfirmDelete = false;
        return deleted;
    }

    /// Genera un nuovo documento della tipologia scelta. Restituisce il documento generato, oppure null in caso di
    /// errore: la pagina naviga solo se c'è qualcosa dove andare.
    public async Task<Document?> GenerateAsync()
    {
        Document? generated = null;

        await RunAsync(async () =>
        {
            generated = await _documentService.GenerateFromAsync(Document!.Id, GenerateType);
        });

        ShowGenerate = false;
        return generated;
    }

    /// Apre la ricerca dei documenti da collegare e ne carica la prima pagina.
    public async Task OpenAttachAsync()
    {
        ShowAttach = true;
        AttachFilter = new DocumentFilter { PageSize = AttachPageSize };

        // si passa dal comando e non dal metodo: chiamandolo direttamente non risulterebbe in esecuzione,
        // e l'indicatore di caricamento non comparirebbe
        await SearchAttachCommand.ExecuteAsync(null);
    }

    /// Ricerca dei documenti candidati al collegamento.
    /// È un comando perché, mentre è in esecuzione, si dichiara non eseguibile: è così che il pulsante di ricerca si
    /// disattiva da solo. Prima restava attivo e permetteva ricerche sovrapposte.
    /// ATTENZIONE: la non eseguibilità non è una guardia automatica. Eseguire il comando mentre è già in corso ne
    /// esegue comunque il corpo — verificato — quindi la protezione sta nel markup che la rispetta.
    /// Il token di annullamento lo fornisce il comando stesso e viene passato alle API.
    [RelayCommand]
    private async Task SearchAttachAsync(CancellationToken cancellationToken)
    {
        AttachError = null;
        AttachFilter.Page = 1;

        try
        {
            _attachResult = await _documentService.FindPagedAsync(AttachFilter, cancellationToken);
        }
        catch (Exception ex) when (ex is ApiException or HttpRequestException)
        {
            _attachResult = new PagedResult<Document>();
            AttachError = ex;
        }
    }

    /// Collega il candidato al documento corrente. Il collegamento vale in entrambi i versi.
    public async Task AttachAsync(Document candidate)
    {
        IsBusy = true;
        AttachError = null;

        try
        {
            IReadOnlyList<DocumentLink> links = await _documentService.AttachAsync(Document!.Id, candidate.Id);
            Document = Document with { LinkedDocuments = links };
            ShowAttach = false;
            Notification = DocumentDetailNotification.Attached;
            NotificationStatus = null;
            await LoadLinkedDocumentsAsync();
        }
        catch (Exception ex) when (ex is ApiException or HttpRequestException)
        {
            AttachError = ex;
        }
        finally
        {
            IsBusy = false;
        }
    }

    // una lettura per ogni collegamento: la risposta porta solo l'identificativo del documento collegato
    private async Task LoadLinkedDocumentsAsync()
    {
        _linkedDocuments.Clear();

        if (Document is null || Document.LinkedDocuments.Count == 0)
        {
            return;
        }

        // lo stato cambia fuori da un gestore di evento, quindi la pagina non si ridisegnerebbe da sola: è la
        // notifica di questo valore, prima acceso e poi spento, a farlo
        IsLoadingLinks = true;

        Document?[] linkedDocuments = await Task.WhenAll(Document.LinkedDocuments
            .Select(link => TryGetDocumentAsync(link.TargetDocumentId)));

        foreach (Document linked in linkedDocuments.OfType<Document>())
        {
            _linkedDocuments[linked.Id] = linked;
        }

        IsLoadingLinks = false;
    }

    private async Task<Document?> TryGetDocumentAsync(string id)
    {
        try
        {
            return await _documentService.GetByIdAsync(id);
        }
        catch (Exception ex) when (ex is ApiException or HttpRequestException)
        {
            return null;
        }
    }

    private void SetDocument(Document document)
    {
        Document = document;
        GenerateType = document.Type switch
        {
            DocumentType.Quote => DocumentType.Proforma,
            _ => DocumentType.SalesOrder
        };
    }

    // ciò che è comune a tutte le azioni: segnalare che si è occupati, azzerare errore e notifica precedenti,
    // e trattare allo stesso modo gli errori previsti
    private async Task<bool> RunAsync(Func<Task> action)
    {
        IsBusy = true;
        Error = null;
        ClearNotification();

        try
        {
            await action();
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
