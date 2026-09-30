namespace WAPIDocManager.UI.Features.Documents.Services;

/// Funzioni di conversione condivise.
/// Le conversioni fra le forme di trasporto e i modelli dell'applicazione sono definite come operatori espliciti sui
/// singoli tipi, e si usano con un cast. Qui resta solo ciò che non appartiene a nessun tipo in particolare.
public static class DocumentMapper
{
    /// Le stringhe vuote o di soli spazi diventano null, le altre vengono ripulite agli estremi.
    /// Serve per i campi di testo inviati alle API: una stringa vuota e un campo assente devono arrivare al server
    /// nello stesso modo, altrimenti si salvano valori vuoti indistinguibili dal non compilato.
    public static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
