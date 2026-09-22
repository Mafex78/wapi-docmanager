using System.Globalization;
using WAPIDocManager.UI.Features.Documents.Entities;
using WAPIDocManager.UI.Shared.Localization;

namespace WAPIDocManager.UI.Shared.Formatting;

/// <summary>
/// Formattazione dei valori mostrati nell'interfaccia secondo la cultura corrente
/// </summary>
/// <remarks>
/// Punto unico per il formato di importi, date e numeri documento: usarlo in tutti i componenti invece di ToString diretti.
/// La cultura corrente è impostata all'avvio da <c>Shared/Localization/AppCultures</c>.
/// </remarks>
public static class DisplayFormat
{
    private const string Empty = "—";

    /// <summary>
    /// Importo con due decimali nel formato della cultura corrente e simbolo € (es. it-IT "1.234,50 €", en-US "1,234.50 €").
    /// </summary>
    public static string Money(decimal amount)
    {
        return $"{amount.ToString("N2", CultureInfo.CurrentCulture)} {DocumentCurrency.Symbol}";
    }

    /// <summary>
    /// Data documento (mezzanotte UTC): mostrata senza conversione di fuso orario
    /// </summary>
    public static string Date(DateTime date)
    {
        return date.ToString("d", CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// Il numero documento è un GUID: in elenco se ne mostrano i primi 8 caratteri
    /// </summary>
    /// <remarks>Il valore completo va mostrato nel dettaglio o come title del link.</remarks>
    public static string ShortNumber(string? number)
    {
        if (string.IsNullOrWhiteSpace(number))
        {
            return Empty;
        }

        return (number.Length > 8 ? number[..8] : number).ToUpperInvariant();
    }

    /// <summary>Testo opzionale: trattino se vuoto.</summary>
    public static string OrDash(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? Empty : value;
    }
}
