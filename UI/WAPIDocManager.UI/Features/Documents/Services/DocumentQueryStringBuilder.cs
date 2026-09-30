using System.Globalization;
using WAPIDocManager.UI.Features.Documents.Models;

namespace WAPIDocManager.UI.Features.Documents.Services;

/// Costruisce la query string della ricerca documenti.
/// I nomi dei parametri devono corrispondere a quelli attesi dal server, che li riconosce senza distinguere
/// maiuscole e minuscole. Le liste viaggiano come parametro ripetuto, una volta per valore, e gli enum come numeri.
/// I parametri opzionali vuoti vengono omessi, e i numeri usano sempre il formato invariante, altrimenti la cultura
/// dell'utente cambierebbe il separatore decimale e il server non capirebbe.
/// L'ordine dei parametri è fisso ed è fissato dai test: non è un requisito del server, ma rende le verifiche
/// leggibili.
public static class DocumentQueryStringBuilder
{
    /// Restituisce la query string senza il punto interrogativo iniziale.
    public static string Build(DocumentFilter filter)
    {
        var parameters = new List<string>
        {
            $"Page={filter.Page.ToString(CultureInfo.InvariantCulture)}",
            $"PageSize={filter.PageSize.ToString(CultureInfo.InvariantCulture)}"
        };

        // la direzione ha senso solo con un campo di ordinamento
        if (!string.IsNullOrWhiteSpace(filter.SortBy))
        {
            parameters.Add($"SortBy={Uri.EscapeDataString(filter.SortBy)}");
            parameters.Add($"SortDirection={(filter.SortDirection == SortDirection.Desc ? "desc" : "asc")}");
        }

        if (!string.IsNullOrWhiteSpace(filter.PlainText))
        {
            parameters.Add($"PlainText={Uri.EscapeDataString(filter.PlainText.Trim())}");
        }

        if (!string.IsNullOrWhiteSpace(filter.CustomerName))
        {
            parameters.Add($"CustomerName={Uri.EscapeDataString(filter.CustomerName.Trim())}");
        }

        // liste: un parametro ripetuto per ogni valore, che è la forma che il server sa ricomporre
        parameters.AddRange(filter.Types
            .Distinct()
            .Select(type => $"DocumentTypes={((int)type).ToString(CultureInfo.InvariantCulture)}"));

        parameters.AddRange(filter.Statuses
            .Distinct()
            .Select(status => $"DocumentStatuses={((int)status).ToString(CultureInfo.InvariantCulture)}"));

        return string.Join("&", parameters);
    }
}
