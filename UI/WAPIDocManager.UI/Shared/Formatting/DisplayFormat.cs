using System.Globalization;
using WAPIDocManager.UI.Features.Documents.Entities;

namespace WAPIDocManager.UI.Shared.Formatting;

/// Formattazione dei valori mostrati nell'interfaccia, secondo la cultura corrente.
/// È il punto unico per importi, date e numeri di documento: va usato in tutti i componenti al posto delle
/// conversioni a stringa fatte sul posto, altrimenti lo stesso valore appare in formati diversi da una pagina
/// all'altra.
public static class DisplayFormat
{
    private const string Empty = "—";

    /// Importo con due decimali nel formato della cultura corrente, seguito dal simbolo di valuta:
    /// in italiano "1.234,50 €", in inglese "1,234.50 €".
    public static string Money(decimal amount)
    {
        return $"{amount.ToString("N2", CultureInfo.CurrentCulture)} {DocumentCurrency.Symbol}";
    }

    /// Data del documento, fissata a mezzanotte UTC: si mostra senza conversione di fuso, altrimenti in alcuni
    /// fusi comparirebbe il giorno prima.
    public static string Date(DateTime date)
    {
        return date.ToString("d", CultureInfo.CurrentCulture);
    }

    /// Il numero del documento è un GUID: negli elenchi se ne mostrano i primi otto caratteri, e il valore
    /// completo va mostrato nel dettaglio o come descrizione del collegamento.
    public static string ShortNumber(string? number)
    {
        if (string.IsNullOrWhiteSpace(number))
        {
            return Empty;
        }

        return (number.Length > 8 ? number[..8] : number).ToUpperInvariant();
    }

    /// Testo opzionale: trattino quando è vuoto.
    public static string OrDash(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? Empty : value;
    }
}
