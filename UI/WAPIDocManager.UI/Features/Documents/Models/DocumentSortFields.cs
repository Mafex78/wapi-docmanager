using WAPIDocManager.UI.Features.Documents.Entities;

namespace WAPIDocManager.UI.Features.Documents.Models;

/// <summary>
/// Campi ordinabili (nomi degli elementi MongoDB della collection documents)
/// </summary>
/// <remarks>
/// <para>
/// Il server ordina con <c>Builders&lt;T&gt;.Sort</c> sul nome passato (<c>Shared.Infrastructure/MongoGenericRepository.cs</c>);
/// <c>WAPIDocument.Infrastructure/DocumentMap.cs</c> usa AutoMap, quindi i nomi sono quelli delle proprietà in PascalCase
/// e i campi annidati usano il punto. Un nome errato non genera errori ma non ordina.
/// </para>
/// <para>
/// Etichette nella UI: chiave risorsa <c>Sort_{nome senza punti}</c> (es. Sort_CustomerName).
/// </para>
/// </remarks>
public static class DocumentSortFields
{
    public const string Date = "Date";
    public const string Number = "Number";
    public const string Total = "Total";
    public const string CustomerName = "Customer.Name";
    public const string Type = "Type";
    public const string Status = "Status";

    /// <summary>Tutti i campi, nell'ordine in cui compaiono nel menu "Ordina per".</summary>
    public static IReadOnlyList<string> All { get; } = new[] { Date, Number, CustomerName, Type, Status, Total };
}
