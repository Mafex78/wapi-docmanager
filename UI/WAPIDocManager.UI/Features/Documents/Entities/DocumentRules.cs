namespace WAPIDocManager.UI.Features.Documents.Entities;

/// Regole del ciclo di vita del documento, speculari a quelle applicate dal server.
/// Il server resta l'autorità: qui servono solo a mostrare le azioni ammesse, senza far tentare all'utente
/// operazioni che verrebbero rifiutate. Se cambiano là vanno aggiornate qui e nei test che le fissano.
/// Una divergenza nota e voluta: il server NON vincola la generazione di un nuovo documento allo stato di quello di
/// partenza, anche se la documentazione lo suggerisce. L'interfaccia segue il comportamento reale, non la
/// documentazione.
public static class DocumentRules
{
    /// La modifica è consentita solo finché il documento è in bozza o pronto.
    public static bool CanEdit(DocumentStatus status)
    {
        return status is DocumentStatus.Draft or DocumentStatus.Ready;
    }

    /// L'eliminazione è consentita solo finché il documento è in bozza o pronto.
    public static bool CanDelete(DocumentStatus status)
    {
        return status is DocumentStatus.Draft or DocumentStatus.Ready;
    }

    /// Stati raggiungibili da quello corrente: da bozza a pronto, poi inviato, e infine approvato o rifiutato.
    /// Non si torna mai in bozza, e approvato e rifiutato sono stati finali.
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

    /// Verifica i requisiti per far avanzare il documento oltre la bozza: cliente con nome e partita IVA, e almeno
    /// una riga valida.
    /// Serve solo a mostrare un avviso preventivo nel dettaglio. Il pulsante resta attivo comunque, e se il server
    /// rifiuta l'avanzamento il suo messaggio viene mostrato all'utente: l'ultima parola resta al server.
    public static bool IsComplete(Document document)
    {
        return !string.IsNullOrWhiteSpace(document.Customer.Name) &&
               !string.IsNullOrWhiteSpace(document.Customer.VatNumber) &&
               document.Lines.Count > 0 &&
               document.Lines.All(line => line.IsValid());
    }
}
