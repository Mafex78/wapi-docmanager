namespace WAPIDocManager.UI.Features.Documents.Models;

/// Direzione di ordinamento.
/// Viaggia come stringa "asc" oppure "desc", e il server ordina in modo decrescente solo riconoscendo la seconda:
/// qualunque altro valore vale come crescente.
public enum SortDirection
{
    Asc = 0,
    Desc = 1
}
