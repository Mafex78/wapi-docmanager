namespace WAPIDocManager.UI.Features.Documents.Entities;

/// Collegamento verso un altro documento.
/// Le API restituiscono soltanto l'identificativo del documento collegato, non il suo numero né il suo stato: è la
/// ragione per cui la pagina di dettaglio esegue una lettura aggiuntiva per ciascun collegamento, per poterne
/// mostrare qualcosa di leggibile.
/// I collegamenti creati a mano sono bidirezionali; quelli creati dalla generazione legano il documento di partenza
/// a quello generato, in entrambi i versi.
public record DocumentLink
{
    public string TargetDocumentId { get; init; } = string.Empty;
}
