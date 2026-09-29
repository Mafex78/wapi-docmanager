namespace WAPIDocManager.UI.Shared.Navigation;

/// <summary>
/// Route dell'applicazione: unica fonte per i template delle pagine e per i link.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="RouteTemplates"/> si usa nelle pagine con <c>@attribute [Route(AppRoutes.RouteTemplates.X)]</c> al posto di <c>@page</c>,
/// che accetta solo stringhe letterali: <c>@page</c> genera proprio un <c>RouteAttribute</c> e il router lo legge allo stesso modo.
/// </para>
/// <para>
/// Le altre costanti e i metodi producono link RELATIVI a <c>&lt;base href&gt;</c> (senza slash iniziale), da usare in
/// <c>href</c>, <c>NavLink</c> e <c>NavigationManager.NavigateTo</c>: così l'app funziona anche se pubblicata sotto un sotto-percorso.
/// </para>
/// <para>
/// Per aggiungere una pagina: template in <see cref="RouteTemplates"/>, costante o metodo per il link qui sotto
/// ed eventuale voce in <c>Layout/NavMenu.razor</c>.
/// </para>
/// </remarks>
public static class AppRoutes
{
    /// <summary>
    /// Template delle pagine: slash iniziale, parametri tra graffe con lo stesso nome della proprietà [Parameter] della pagina.
    /// </summary>
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

    /// <summary>Dettaglio documento (Id codificato per l'URL).</summary>
    public static string GetDocumentDetail(string id) => $"{PageRelativeUrls.Documents}/{Uri.EscapeDataString(id)}";

    /// <summary>
    /// Modifica documento (Id codificato per l'URL)
    /// </summary>
    /// <param name="id">ID del documento</param>
    /// <param name="returnUrl">
    /// Pagina a cui tornare con Indietro/Annulla, quando la modifica è aperta da un punto diverso dal dettaglio
    /// (es. dalla lista). Omesso: la modifica torna al dettaglio del documento.
    /// Il valore è validato da <see cref="SafeReturnUrl"/> prima dell'uso, come il returnUrl del login.
    /// </param>
    public static string GetDocumentEdit(string id, string? returnUrl = null)
    {
        string url = $"{GetDocumentDetail(id)}/edit";

        return string.IsNullOrEmpty(returnUrl)
            ? url
            : $"{url}?returnUrl={Uri.EscapeDataString(returnUrl)}";
    }

    /// <summary>
    /// Login con l'indirizzo a cui tornare dopo l'accesso (senza parametro se vuoto).
    /// </summary>
    /// <remarks>Il valore viene validato da <c>Features/Auth/Views/Pages/Login.razor</c> prima di essere usato (protezione open redirect).</remarks>
    public static string LoginWithReturnUrl(string? returnUrl)
    {
        return string.IsNullOrEmpty(returnUrl)
            ? PageRelativeUrls.Login
            : $"{PageRelativeUrls.Login}?returnUrl={Uri.EscapeDataString(returnUrl)}";
    }
}
