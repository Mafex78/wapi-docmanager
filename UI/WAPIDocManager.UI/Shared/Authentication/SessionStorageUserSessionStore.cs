using System.Text.Json;
using Microsoft.JSInterop;
using WAPIDocManager.UI.Shared.Api;

namespace WAPIDocManager.UI.Shared.Authentication;

/// Sessione salvata nell'archivio di sessione del browser, con una copia in memoria.
/// L'archivio di sessione è una scelta concordata: la sessione sopravvive al ricaricamento della pagina, e quindi
/// anche al cambio lingua, ma non è condivisa fra schede e termina quando la scheda viene chiusa. Per passare
/// all'archivio permanente basta cambiare gli identificatori JavaScript usati qui sotto.
/// Le funzioni di lettura e scrittura sono globali del browser: non serve codice JavaScript nostro.
public class SessionStorageUserSessionStore : IUserSessionStore
{
    /// Chiave nell'archivio di sessione, visibile dagli strumenti di sviluppo del browser.
    public const string StorageKey = "wapidocmanager.session";

    private readonly IJSRuntime _jsRuntime;

    // la copia in memoria evita una chiamata JavaScript a ogni richiesta HTTP: la sessione viene letta prima di
    // ogni chiamata alle API, per aggiungere il token
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

    // valore assente o non più compatibile, per esempio dopo una modifica delle proprietà salvate = nessuna sessione
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
