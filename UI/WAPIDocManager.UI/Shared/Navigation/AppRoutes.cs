namespace WAPIDocManager.UI.Shared.Navigation;

/// Indirizzi dell'applicazione: unica fonte sia per le rotte delle pagine sia per i collegamenti che le raggiungono.
/// Le rotte si dichiarano nelle pagine con l'attributo, e non con la direttiva, perché quella accetta soltanto
/// stringhe letterali: per il router il risultato è identico.
/// Le altre costanti e i metodi producono collegamenti RELATIVI all'indirizzo base, senza slash iniziale: così
/// l'applicazione continua a funzionare anche se pubblicata sotto un sotto-percorso.
/// Per aggiungere una pagina servono la rotta qui sotto, la costante o il metodo che costruisce il collegamento, ed
/// eventualmente una voce nel menu laterale.
public static class AppRoutes
{
    /// Rotte delle pagine: slash iniziale, e parametri fra graffe con lo stesso nome della proprietà che li riceve.
    public static class RouteTemplates
    {
        public const string Home = "/";
        public const string Login = $"/{PageRelativeUrls.Login}";
        public const string Documents = $"/{PageRelativeUrls.Documents}";
        public const string DocumentNew = $"/{PageRelativeUrls.DocumentNew}";
        public const string DocumentDetail = $"/{PageRelativeUrls.Documents}/{{Id}}";
        public const string DocumentEdit = $"/{PageRelativeUrls.Documents}/{{Id}}/edit";
        public const string UserRegister = $"/{PageRelativeUrls.UserRegister}";
    }

    // Definisce le stringhe "pulite" (senza slash) come costanti di base
    public static class PageRelativeUrls
    {
        public const string Login = "login";
        public const string Documents = "documents";
        public const string DocumentNew = $"{Documents}/new";
        public const string UserRegister = "users/register";
    }

    /// Dettaglio di un documento, con l'identificativo codificato per l'indirizzo.
    public static string GetDocumentDetail(string id) => $"{PageRelativeUrls.Documents}/{Uri.EscapeDataString(id)}";

    /// Modifica di un documento.
    /// L'indirizzo di ritorno serve quando la modifica viene aperta da un punto diverso dal dettaglio, per esempio
    /// dall'elenco; omesso, Indietro riporta al dettaglio del documento. Viene validato prima dell'uso, perché
    /// arriva dalla query string.
    public static string GetDocumentEdit(string id, string? returnUrl = null)
    {
        string url = $"{GetDocumentDetail(id)}/edit";

        return string.IsNullOrEmpty(returnUrl)
            ? url
            : $"{url}?returnUrl={Uri.EscapeDataString(returnUrl)}";
    }

    /// Pagina di accesso con l'indirizzo a cui tornare una volta entrati, omesso se l'indirizzo è vuoto.
    /// Anche questo valore viene validato prima dell'uso, per non rimbalzare l'utente fuori dall'applicazione.
    public static string LoginWithReturnUrl(string? returnUrl)
    {
        return string.IsNullOrEmpty(returnUrl)
            ? PageRelativeUrls.Login
            : $"{PageRelativeUrls.Login}?returnUrl={Uri.EscapeDataString(returnUrl)}";
    }
}
