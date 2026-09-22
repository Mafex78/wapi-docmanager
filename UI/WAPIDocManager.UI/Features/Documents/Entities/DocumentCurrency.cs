using WAPIDocManager.UI.Features.Documents.Contracts;
using WAPIDocManager.UI.Shared.Formatting;

namespace WAPIDocManager.UI.Features.Documents.Entities;

/// <summary>
/// Valuta gestita dal client: cablata a Euro (decisione di progetto).
/// </summary>
/// <remarks>
/// <para>
/// Motivo: <c>DocumentReadResponse</c> di WAPIDocument (GET) non restituisce il campo Currency, quindi il client
/// non potrebbe precaricarla nel form di modifica.
/// </para>
/// <para>
/// Il client invia SEMPRE <see cref="Code"/> in creazione e modifica (vedi gli operatori espliciti di <c>Features/Documents/Contracts/DocumentCreateRequest</c> e <c>DocumentUpdateRequest</c>) perché:
/// il PUT sovrascrive la valuta (se vuota verrebbe azzerata) e il dominio backend rifiuta il passaggio a Ready/Sent senza valuta.
/// Documenti creati fuori dalla UI con altre valute verrebbero comunque mostrati in €.
/// </para>
/// </remarks>
public static class DocumentCurrency
{
    /// <summary>Codice ISO inviato alle API.</summary>
    public const string Code = "EUR";

    /// <summary>Simbolo mostrato accanto agli importi (vedi <c>Shared/Formatting/DisplayFormat.Money</c> nel progetto UI).</summary>
    public const string Symbol = "€";
}
