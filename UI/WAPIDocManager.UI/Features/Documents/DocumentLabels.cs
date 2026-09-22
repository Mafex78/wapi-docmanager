using Microsoft.Extensions.Localization;
using WAPIDocManager.UI.Features.Documents.Entities;
using WAPIDocManager.UI.Features.Documents.Models;
using WAPIDocManager.UI.Shared.Localization;
using WAPIDocManager.UI;

namespace WAPIDocManager.UI.Features.Documents;

/// <summary>
/// Etichette localizzate degli enum e dei campi di ordinamento dello slice Documenti
/// </summary>
/// <remarks>
/// <para>
/// Stanno qui e non in <c>Shared/Localization/LocalizerExtensions.cs</c> perché parlano di tipi dello slice
/// (<see cref="DocumentType"/>, <see cref="DocumentStatus"/>, <see cref="SortDirection"/>): in Shared va solo
/// ciò che serve a due o più funzionalità, altrimenti il codice condiviso torna a dipendere dagli slice.
/// </para>
/// <para>
/// Le chiavi sono costruite dal nome del valore (es. <c>DocumentStatus_Draft</c>): aggiungendo o rinominando un valore
/// di un enum va aggiunta o rinominata la chiave in ENTRAMBI i file <c>Resources/SharedResource*.resx</c>.
/// Una chiave mancante viene mostrata così com'è.
/// </para>
/// <para>
/// Sono metodi di estensione di <see cref="IStringLocalizer"/>: nelle pagine dello slice si usano come
/// <c>@L.Label(documento.Type)</c> senza using aggiuntivi, perché vivono nel namespace della funzionalità.
/// </para>
/// </remarks>
public static class DocumentLabels
{
    /// <summary>Chiave <c>DocumentType_{Valore}</c>.</summary>
    public static string Label(this IStringLocalizer localizer, DocumentType type)
    {
        return localizer[$"DocumentType_{type}"];
    }

    /// <summary>Chiave <c>DocumentStatus_{Valore}</c>.</summary>
    public static string Label(this IStringLocalizer localizer, DocumentStatus status)
    {
        return localizer[$"DocumentStatus_{status}"];
    }

    /// <summary>Chiave <c>SortDirection_{Valore}</c>.</summary>
    public static string Label(this IStringLocalizer localizer, SortDirection direction)
    {
        return localizer[$"SortDirection_{direction}"];
    }

    /// <summary>Chiave <c>Sort_{campo senza punti}</c> (es. "Customer.Name" → Sort_CustomerName), vedi <see cref="DocumentSortFields"/>.</summary>
    public static string SortFieldLabel(this IStringLocalizer localizer, string field)
    {
        return localizer[$"Sort_{field.Replace(".", string.Empty)}"];
    }

    /// <summary>
    /// Testo del pulsante che porta il documento nello stato indicato
    /// </summary>
    /// <remarks>Chiave <c>StatusAction_{Valore}</c> (es. "Segna come pronto").</remarks>
    public static string StatusActionLabel(this IStringLocalizer localizer, DocumentStatus targetStatus)
    {
        return localizer[$"StatusAction_{targetStatus}"];
    }
}
