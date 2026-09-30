using Microsoft.Extensions.Localization;
using WAPIDocManager.UI.Features.Documents.Entities;
using WAPIDocManager.UI.Features.Documents.Models;

namespace WAPIDocManager.UI.Features.Documents;

/// Etichette tradotte degli enum e dei campi di ordinamento di questa funzionalità.
/// Stanno qui e non fra i componenti condivisi perché parlano di tipi che appartengono a questa funzionalità: là
/// va solo ciò che serve a due o più funzionalità, altrimenti il codice condiviso torna a dipendere dalle singole
/// parti dell'applicazione.
/// Le chiavi sono costruite dal nome del valore: aggiungendo o rinominando un valore di un enum va aggiunta o
/// rinominata la chiave in ENTRAMBI i file di risorse. Una chiave mancante viene mostrata all'utente così com'è.
/// Sono metodi di estensione del traduttore, e vivono nel namespace della funzionalità: nelle pagine si usano
/// direttamente, senza dichiarazioni aggiuntive.
public static class DocumentLabels
{
    public static string Label(this IStringLocalizer localizer, DocumentType type)
    {
        return localizer[$"DocumentType_{type}"];
    }

    public static string Label(this IStringLocalizer localizer, DocumentStatus status)
    {
        return localizer[$"DocumentStatus_{status}"];
    }

    public static string Label(this IStringLocalizer localizer, SortDirection direction)
    {
        return localizer[$"SortDirection_{direction}"];
    }

    /// I punti nel nome del campo vengono tolti per formare la chiave: "Customer.Name" diventa "Sort_CustomerName".
    public static string SortFieldLabel(this IStringLocalizer localizer, string field)
    {
        return localizer[$"Sort_{field.Replace(".", string.Empty)}"];
    }

    /// Testo del pulsante che porta il documento nello stato indicato, per esempio "Segna come pronto": è un invito
    /// ad agire, non il nome dello stato, e ha quindi una chiave diversa dall'etichetta.
    public static string StatusActionLabel(this IStringLocalizer localizer, DocumentStatus targetStatus)
    {
        return localizer[$"StatusAction_{targetStatus}"];
    }
}
