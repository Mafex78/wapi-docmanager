using WAPIDocManager.UI.Features.Documents.Models;

namespace WAPIDocManager.UI.Features.Documents;

/// Conserva i filtri dell'elenco documenti durante la navigazione, così tornando dal dettaglio si ritrova la ricerca
/// appena fatta invece di un elenco azzerato.
/// Vive quanto la scheda del browser, a differenza dei ViewModel che nascono a ogni visita: il form dell'elenco è
/// legato direttamente a questa istanza. Si perde ricaricando la pagina, e quindi anche al cambio di lingua.
public class DocumentListState
{
    public DocumentFilter Filter { get; set; } = new();
}
