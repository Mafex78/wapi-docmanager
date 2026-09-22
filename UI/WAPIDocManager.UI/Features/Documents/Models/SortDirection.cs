using WAPIDocManager.UI.Features.Documents.Services;

namespace WAPIDocManager.UI.Features.Documents.Models;

/// <summary>
/// Direzione di ordinamento.
/// </summary>
/// <remarks>
/// Inviata come stringa "asc"/"desc" (<c>DocumentQueryStringBuilder</c>); il server ordina in modo decrescente
/// solo se il valore è "desc" (case-insensitive). Etichette: chiave risorsa <c>SortDirection_{Valore}</c>.
/// </remarks>
public enum SortDirection
{
    Asc = 0,
    Desc = 1
}
