namespace WAPIDocManager.UI.Features.Documents.Models;

/// Campi su cui si può ordinare la ricerca.
/// I nomi non sono liberi: devono coincidere con quelli dei campi memorizzati sul server, che corrispondono alle
/// proprietà scritte in PascalCase, con il punto per scendere dentro un oggetto annidato. Un nome sbagliato non
/// produce alcun errore, semplicemente non ordina — ed è la ragione per cui esistono queste costanti invece di
/// stringhe sparse.
public static class DocumentSortFields
{
    public const string Date = "Date";
    public const string Number = "Number";
    public const string Total = "Total";
    public const string CustomerName = "Customer.Name";
    public const string Type = "Type";
    public const string Status = "Status";

    /// Tutti i campi, nell'ordine in cui compaiono nel menu di ordinamento.
    public static IReadOnlyList<string> All { get; } = new[] { Date, Number, CustomerName, Type, Status, Total };
}
