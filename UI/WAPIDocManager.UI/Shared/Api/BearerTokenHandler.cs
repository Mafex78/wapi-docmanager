using System.Net.Http.Headers;
using System.Net;
using WAPIDocManager.UI.Features.Documents.Services;
using WAPIDocManager.UI.Features.Users.Services;
using WAPIDocManager.UI.Shared.Authentication;

namespace WAPIDocManager.UI.Shared.Api;

/// <summary>
/// Aggiunge il JWT alle richieste verso le API protette e termina la sessione se il token è scaduto o rifiutato (401)
/// </summary>
/// <remarks>
/// <para>
/// Registrato (ServiceCollectionExtensions) sui typed HttpClient di <c>IDocumentService</c> e <c>IUserService</c>,
/// NON su quello del login. Viene risolto da IHttpClientFactory in uno scope DI separato: per questo
/// <see cref="IUserSessionStore"/> deve essere Singleton, altrimenti leggerebbe una sessione diversa da quella della UI.
/// </para>
/// <para>
/// Pulire la sessione solleva <c>SessionChanged</c>: lo stato di autenticazione diventa anonimo e la UI
/// reindirizza automaticamente al login (AuthorizeRouteView → RedirectToLogin).
/// </para>
/// </remarks>
public class BearerTokenHandler : DelegatingHandler
{
    private readonly IUserSessionStore _sessionStore;
    private readonly TimeProvider _timeProvider;

    public BearerTokenHandler(
        IUserSessionStore sessionStore,
        TimeProvider timeProvider)
    {
        _sessionStore = sessionStore;
        _timeProvider = timeProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        UserSession? session = await _sessionStore.GetAsync();

        if (session is not null)
        {
            // token già scaduto: inutile chiamare l'API (risponderebbe 401), si chiude subito la sessione
            if (session.IsExpired(_timeProvider.GetUtcNow().UtcDateTime))
            {
                await _sessionStore.ClearAsync();
                throw new ApiException(HttpStatusCode.Unauthorized, null);
            }

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.Token);
        }

        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

        // 401 = token rifiutato (es. chiave di firma cambiata): logout.
        // 403 = ruolo insufficiente: la sessione resta valida e l'errore viene mostrato dalla pagina.
        if (response.StatusCode == HttpStatusCode.Unauthorized && session is not null)
        {
            await _sessionStore.ClearAsync();
        }

        return response;
    }
}
