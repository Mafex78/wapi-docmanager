namespace WAPIDocManager.UI.Features.Documents.Entities;

/// <summary>
/// Regole del ciclo di vita del documento, speculari all'aggregato Document di WAPIDocument.
/// Il server resta l'autorità: il client le usa solo per mostrare le azioni ammesse.
/// </summary>
/// <remarks>
/// <para>
/// Sorgente delle regole: <c>WAPIDocument.Domain/Entities/Documents/Document.cs</c>
/// (CanUpdate, CanDelete, UpdateStatus/SetStatus, Validate). Se cambiano lì vanno aggiornate qui
/// e nei test <c>Tests/WAPIDocManager.UI.Tests/Features/Documents/DocumentRulesTests.cs</c>.
/// </para>
/// <para>
/// Nota: il backend NON vincola la generazione di un nuovo documento allo stato del sorgente
/// (il README parla di Approved, ma il codice non lo verifica): la UI segue il codice.
/// </para>
/// </remarks>
public static class DocumentRules
{
    /// <summary>La modifica (PUT) è consentita solo in Draft e Ready.</summary>
    public static bool CanEdit(DocumentStatus status)
    {
        return status is DocumentStatus.Draft or DocumentStatus.Ready;
    }

    /// <summary>L'eliminazione (DELETE) è consentita solo in Draft e Ready.</summary>
    public static bool CanDelete(DocumentStatus status)
    {
        return status is DocumentStatus.Draft or DocumentStatus.Ready;
    }

    /// <summary>
    /// Stati raggiungibili dallo stato corrente: Draft → Ready → Sent → Approved | Rejected.
    /// </summary>
    /// <remarks>
    /// Draft non è mai un target valido (rifiutato da <c>DocumentChangeStatusValidator</c>); Approved e Rejected sono stati finali.
    /// </remarks>
    public static IReadOnlyList<DocumentStatus> GetNextStatuses(DocumentStatus status)
    {
        return status switch
        {
            DocumentStatus.Draft => new[] { DocumentStatus.Ready },
            DocumentStatus.Ready => new[] { DocumentStatus.Sent },
            DocumentStatus.Sent => new[] { DocumentStatus.Approved, DocumentStatus.Rejected },
            _ => Array.Empty<DocumentStatus>()
        };
    }

    /// <summary>
    /// Verifica i requisiti per portare il documento in stato Ready o Sent
    /// (cliente con Name e VatNumber, almeno una riga valida; la valuta è sempre EUR)
    /// </summary>
    /// <remarks>
    /// Usata solo per mostrare un avviso preventivo nella pagina di dettaglio: il pulsante resta attivo
    /// e l'eventuale errore restituito dal server (400) viene mostrato all'utente.
    /// </remarks>
    public static bool IsComplete(Document document)
    {
        return !string.IsNullOrWhiteSpace(document.Customer.Name) &&
               !string.IsNullOrWhiteSpace(document.Customer.VatNumber) &&
               document.Lines.Count > 0 &&
               document.Lines.All(line => line.IsValid());
    }
}
