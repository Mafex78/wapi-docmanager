using WAPIDocManager.UI.Features.Documents.Views.Pages;

namespace WAPIDocManager.UI.Shared.Navigation;

/// <summary>
/// Route dell'applicazione: unica fonte per i template delle pagine e per i link.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Templates"/> si usa nelle pagine con <c>@attribute [Route(AppRoutes.Templates.X)]</c> al posto di <c>@page</c>,
/// che accetta solo stringhe letterali: <c>@page</c> genera proprio un <c>RouteAttribute</c> e il router lo legge allo stesso modo.
/// </para>
/// <para>
/// Le altre costanti e i metodi producono link RELATIVI a <c>&lt;base href&gt;</c> (senza slash iniziale), da usare in
/// <c>href</c>, <c>NavLink</c> e <c>NavigationManager.NavigateTo</c>: così l'app funziona anche se pubblicata sotto un sotto-percorso.
/// </para>
/// <para>
/// Per aggiungere una pagina: template in <see cref="Templates"/>, costante o metodo per il link qui sotto
/// ed eventuale voce in <c>Layout/NavMenu.razor</c>.
/// </para>
/// </remarks>
public static class AppRoutes
{
    /// <summary>
    /// Template delle pagine: slash iniziale, parametri tra graffe con lo stesso nome della proprietà [Parameter] della pagina.
    /// </summary>
    public static class Templates
    {
        public const string Home = "/";
        public const string Login = "/login";
        public const string Documents = "/documents";
        public const string DocumentNew = "/documents/new";
        public const string DocumentDetail = "/documents/{Id}";
        public const string DocumentEdit = "/documents/{Id}/edit";
        public const string UserRegister = "/users/register";
    }

    public const string Login = "login";
    public const string Documents = "documents";
    public const string DocumentNew = "documents/new";
    public const string UserRegister = "users/register";

    /// <summary>Dettaglio documento (Id codificato per l'URL).</summary>
    public static string DocumentDetail(string id)
    {
        return $"{Documents}/{Uri.EscapeDataString(id)}";
    }

    /// <summary>Modifica documento (Id codificato per l'URL).</summary>
    public static string DocumentEdit(string id)
    {
        return $"{DocumentDetail(id)}/edit";
    }

    /// <summary>
    /// Login con l'indirizzo a cui tornare dopo l'accesso (senza parametro se vuoto).
    /// </summary>
    /// <remarks>Il valore viene validato da <c>Features/Auth/Views/Pages/Login.razor</c> prima di essere usato (protezione open redirect).</remarks>
    public static string LoginWithReturnUrl(string? returnUrl)
    {
        return string.IsNullOrEmpty(returnUrl)
            ? Login
            : $"{Login}?returnUrl={Uri.EscapeDataString(returnUrl)}";
    }
}
