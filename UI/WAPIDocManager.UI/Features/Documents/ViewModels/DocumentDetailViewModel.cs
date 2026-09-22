using WAPIDocManager.UI.Features.Documents.Entities;
using WAPIDocManager.UI.Features.Documents.Models;
using WAPIDocManager.UI.Features.Documents.Services;
using WAPIDocManager.UI.Features.Documents.Views.Pages;
using WAPIDocManager.UI.Shared.Api;

namespace WAPIDocManager.UI.Features.Documents.ViewModels;

/// <summary>
/// Logica della pagina di dettaglio documento (MVVM): caricamento, avanzamento di stato, generazione,
/// collegamento, eliminazione e caricamento dei documenti collegati.
/// </summary>
/// <remarks>
/// <para>
/// Usato da <c>WAPIDocManager.UI/Features/Documents/Views/Pages/DocumentDetail.razor</c>. Restano nel componente:
/// ruoli e permessi, testi localizzati (vedi <see cref="Notification"/>) e la navigazione, che avviene in base
/// all'esito di <see cref="DeleteAsync"/> e <see cref="GenerateAsync"/>.
/// </para>
/// <para>
/// Registrato Transient: la navigazione tra documenti riusa la stessa istanza del componente, quindi il ricaricamento
/// è governato dalla guardia sull'Id in <see cref="LoadAsync"/>.
/// </para>
/// </remarks>
public sealed class DocumentDetailViewModel
{
    /// <summary>Risultati per pagina nella ricerca della modale di collegamento.</summary>
    public const int AttachPageSize = 10;

    private readonly IDocumentService _documentService;
    private readonly Dictionary<string, Document> _linkedDocuments = new();

    private PagedResult<Document> _attachResult = new();
    private string? _loadedId;

    public DocumentDetailViewModel(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    /// <summary>
    /// Sollevato quando lo stato cambia fuori dal gestore di evento della pagina e serve un ridisegno
    /// (caricamento dei documenti collegati). La pagina si iscrive e chiama StateHasChanged.
    /// </summary>
    public event Action? StateChanged;

    public Document? Document { get; private set; }

    /// <summary>Errore dell'ultima azione; la pagina lo azzera quando l'utente chiude l'avviso.</summary>
    public Exception? Error { get; set; }

    public DocumentDetailNotification Notification { get; private set; }

    /// <summary>Stato raggiunto, valorizzato solo con <see cref="DocumentDetailNotification.StatusUpdated"/>.</summary>
    public DocumentStatus? NotificationStatus { get; private set; }

    public bool IsLoading { get; private set; } = true;

    public bool IsLoadingLinks { get; private set; }

    /// <summary>Azione in corso: la pagina disabilita i pulsanti.</summary>
    public bool IsBusy { get; private set; }

    public bool ConfirmDelete { get; set; }

    public bool ShowGenerate { get; set; }

    /// <summary>Tipologia proposta per la generazione (modificabile dalla modale).</summary>
    public DocumentType GenerateType { get; set; }

    public bool ShowAttach { get; set; }

    public bool AttachLoading { get; private set; }

    public Exception? AttachError { get; set; }

    public DocumentFilter AttachFilter { get; private set; } = new() { PageSize = AttachPageSize };

    /// <summary>
    /// Candidati al collegamento: esclude il documento corrente e quelli già collegati.
    /// </summary>
    /// <remarks>
    /// Metodo e non proprietà (S2365): a ogni chiamata filtra i risultati della ricerca e costruisce una nuova lista,
    /// quindi il costo deve essere evidente a chi lo usa dal markup.
    /// </remarks>
    public IReadOnlyList<Document> GetAttachCandidates()
    {
        return _attachResult.Items
            .Where(candidate => candidate.Id != Document?.Id &&
                                Document?.LinkedDocuments.Any(link => link.TargetDocumentId == candidate.Id) != true)
            .ToList();
    }

    /// <summary>Documento collegato già letto dalle API, null se non disponibile (es. eliminato).</summary>
    public Document? TryGetLinkedDocument(string id)
    {
        return _linkedDocuments.GetValueOrDefault(id);
    }

    /// <summary>
    /// Righe dei documenti collegati: per ogni link, il documento già letto oppure null se non disponibile.
    /// </summary>
    /// <remarks>
    /// Il markup itera su queste coppie invece di chiamare <see cref="TryGetLinkedDocument"/> dentro il ciclo:
    /// il recupero resta nel ViewModel e la pagina si limita a mostrare (S3267).
    /// </remarks>
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

    /// <summary>
    /// Carica il documento e i suoi collegati. Ricarica solo se l'Id è cambiato: la pagina è riusata
    /// quando si passa da un documento all'altro.
    /// </summary>
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

    /// <summary>Elimina il documento corrente.</summary>
    /// <returns>true se eliminato: la pagina torna alla lista.</returns>
    public async Task<bool> DeleteAsync()
    {
        bool deleted = await RunAsync(async () => await _documentService.DeleteAsync(Document!.Id));

        ConfirmDelete = false;
        return deleted;
    }

    /// <summary>Genera un nuovo documento della tipologia in <see cref="GenerateType"/>.</summary>
    /// <returns>Il documento generato, oppure null in caso di errore: la pagina naviga solo se non è null.</returns>
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

    /// <summary>Apre la modale di collegamento e carica la prima pagina di candidati.</summary>
    public async Task OpenAttachAsync()
    {
        ShowAttach = true;
        AttachFilter = new DocumentFilter { PageSize = AttachPageSize };
        await SearchAttachAsync();
    }

    public async Task SearchAttachAsync()
    {
        AttachLoading = true;
        AttachError = null;
        AttachFilter.Page = 1;

        try
        {
            _attachResult = await _documentService.FindPagedAsync(AttachFilter);
        }
        catch (Exception ex) when (ex is ApiException or HttpRequestException)
        {
            _attachResult = new PagedResult<Document>();
            AttachError = ex;
        }
        finally
        {
            AttachLoading = false;
        }
    }

    /// <summary>Collega il candidato al documento corrente (collegamento bidirezionale lato server).</summary>
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

    // un GET per ogni link: il DTO dei collegamenti contiene solo l'Id
    private async Task LoadLinkedDocumentsAsync()
    {
        _linkedDocuments.Clear();

        if (Document is null || Document.LinkedDocuments.Count == 0)
        {
            return;
        }

        IsLoadingLinks = true;

        // qui lo stato cambia fuori da un gestore di evento: la pagina deve ridisegnarsi per mostrare lo spinner
        StateChanged?.Invoke();

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

    // busy, azzeramento di errore e notifica, gestione degli errori previsti: comuni a tutte le azioni
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
