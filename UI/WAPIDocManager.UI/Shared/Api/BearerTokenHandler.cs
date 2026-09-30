using System.Net.Http.Headers;
using System.Net;
using WAPIDocManager.UI.Shared.Authentication;

namespace WAPIDocManager.UI.Shared.Api;

/// Allega il token alle richieste verso le API protette e chiude la sessione quando il token è scaduto o viene
/// rifiutato. Non è montato sul client dell'accesso, che chiama un endpoint anonimo.
/// Viene risolto in uno scope di dipendenze separato da quello dei componenti: è la ragione per cui la sessione
/// va registrata Singleton, altrimenti qui si leggerebbe una sessione diversa da quella vista dall'interfaccia.
/// Chiudere la sessione rende anonimo lo stato di autenticazione, e l'utente finisce alla pagina di accesso senza
/// che nessuno debba navigare a mano.
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
            // token già scaduto: inutile chiamare l'API, risponderebbe 401. Si chiude subito la sessione
            if (session.IsExpired(_timeProvider.GetUtcNow().UtcDateTime))
            {
                await _sessionStore.ClearAsync();
                throw new ApiException(HttpStatusCode.Unauthorized, null);
            }

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.Token);
        }

        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

        // 401 = token rifiutato, per esempio perché la chiave di firma è cambiata: si chiude la sessione.
        // 403 = ruolo insufficiente: la sessione resta valida e l'errore lo mostra la pagina.
        if (response.StatusCode == HttpStatusCode.Unauthorized && session is not null)
        {
            await _sessionStore.ClearAsync();
        }

        return response;
    }
}
