using WAPIDocManager.UI.Features.Documents.Models;
using WAPIDocManager.UI.Features.Documents.ViewModels;

namespace WAPIDocManager.UI.Features.Documents;

/// <summary>
/// Mantiene i filtri della lista documenti durante la navigazione (dettaglio → lista)
/// </summary>
/// <remarks>
/// Registrato Scoped in Program.cs (in WebAssembly vive quanto la scheda), a differenza dei ViewModel che sono Transient:
/// <c>DocumentListViewModel</c> legge e sostituisce <see cref="Filter"/>, e il form della lista è legato
/// direttamente a quell'istanza. Si perde con il reload della pagina (anche al cambio lingua).
/// </remarks>
public class DocumentListState
{
    public DocumentFilter Filter { get; set; } = new();
}
