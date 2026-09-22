using System.Globalization;
using WAPIDocManager.UI.Features.Documents.Models;

namespace WAPIDocManager.UI.Features.Documents.Services;

/// <summary>
/// Query string di GET api/v1/documents (binding [FromQuery] di DocumentFindPagedByFilterRequest:
/// le liste sono parametri ripetuti, gli enum valori numerici)
/// </summary>
/// <remarks>
/// <para>
/// I nomi dei parametri sono i nomi delle proprietà di <c>DocumentFindPagedByFilterRequest</c> / <c>FilterPagingDto</c>
/// (il model binding è case-insensitive). Esempio: <c>Page=1&amp;PageSize=20&amp;SortBy=Date&amp;SortDirection=desc&amp;DocumentTypes=0&amp;DocumentTypes=2</c>.
/// </para>
/// <para>
/// L'ordine dei parametri è fisso ed è verificato dai test (<c>DocumentQueryStringBuilderTests</c>, <c>DocumentApiServiceTests</c>).
/// I parametri opzionali vuoti vengono omessi; i valori numerici usano la cultura invariante.
/// </para>
/// </remarks>
public static class DocumentQueryStringBuilder
{
    /// <summary>Restituisce la query string senza il "?" iniziale.</summary>
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

        // liste: un parametro ripetuto per ogni valore (binding di IList<enum> in ASP.NET Core)
        parameters.AddRange(filter.Types
            .Distinct()
            .Select(type => $"DocumentTypes={((int)type).ToString(CultureInfo.InvariantCulture)}"));

        parameters.AddRange(filter.Statuses
            .Distinct()
            .Select(status => $"DocumentStatuses={((int)status).ToString(CultureInfo.InvariantCulture)}"));

        return string.Join("&", parameters);
    }
}
