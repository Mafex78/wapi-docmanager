using System.Text.Json;
using Microsoft.JSInterop;
using WAPIDocManager.UI.Shared.Api;

namespace WAPIDocManager.UI.Shared.Authentication;

/// <summary>
/// Sessione utente salvata nel sessionStorage del browser, con cache in memoria.
/// Va registrata Singleton: i DelegatingHandler di IHttpClientFactory vengono risolti in uno scope separato.
/// </summary>
/// <remarks>
/// <para>
/// sessionStorage (scelta concordata) = la sessione sopravvive al reload (anche al cambio lingua) ma non è condivisa
/// tra schede e termina alla chiusura della scheda. Per passare a localStorage basta cambiare gli identificatori JS.
/// </para>
/// <para>
/// Le funzioni <c>sessionStorage.getItem/setItem/removeItem</c> sono globali del browser: non serve codice in wwwroot/js/app.js.
/// Il valore è <see cref="UserSession"/> serializzato con <see cref="JsonDefaults"/>.
/// </para>
/// </remarks>
public class SessionStorageUserSessionStore : IUserSessionStore
{
    /// <summary>Chiave nel sessionStorage (visibile dagli strumenti del browser).</summary>
    public const string StorageKey = "wapidocmanager.session";

    private readonly IJSRuntime _jsRuntime;

    // cache: evita una chiamata JS interop a ogni richiesta HTTP (il BearerTokenHandler legge la sessione ogni volta)
    private UserSession? _session;
    private bool _loaded;

    public SessionStorageUserSessionStore(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <inheritdoc />
    public event Action<UserSession?>? SessionChanged;

    public async ValueTask<UserSession?> GetAsync()
    {
        if (_loaded)
        {
            return _session;
        }

        string? json = await _jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", StorageKey);

        _session = Deserialize(json);
        _loaded = true;

        return _session;
    }

    public async ValueTask SetAsync(UserSession session)
    {
        await _jsRuntime.InvokeVoidAsync(
            "sessionStorage.setItem",
            StorageKey,
            JsonSerializer.Serialize(session, JsonDefaults.Options));

        _session = session;
        _loaded = true;

        SessionChanged?.Invoke(session);
    }

    public async ValueTask ClearAsync()
    {
        await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", StorageKey);

        _session = null;
        _loaded = true;

        SessionChanged?.Invoke(null);
    }

    // valore assente o non più compatibile (es. dopo una modifica di UserSession) = nessuna sessione
    private static UserSession? Deserialize(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<UserSession>(json, JsonDefaults.Options);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
