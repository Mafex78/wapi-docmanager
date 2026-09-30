namespace WAPIDocManager.UI.Features.Documents.Entities;

/// Valuta gestita dal client, fissata all'euro per decisione di progetto.
/// La ragione è che le API non restituiscono la valuta quando si legge un documento: il client non potrebbe quindi
/// precaricarla nel form di modifica.
/// Il codice viene però inviato SEMPRE, in creazione e in modifica, per due motivi: la modifica sovrascrive l'intero
/// documento, quindi una valuta vuota la azzererebbe, e il server rifiuta l'avanzamento di stato di un documento
/// senza valuta.
/// Conseguenza accettata: un documento creato fuori da questa interfaccia con un'altra valuta verrebbe comunque
/// mostrato in euro.
public static class DocumentCurrency
{
    /// Codice inviato alle API.
    public const string Code = "EUR";

    /// Simbolo mostrato accanto agli importi.
    public const string Symbol = "€";
}
