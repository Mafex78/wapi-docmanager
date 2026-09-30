using System.Net.Http.Json;
using WAPIDocManager.UI.Features.Auth.Models;
using WAPIDocManager.UI.Features.Auth.Services.Dto;
using WAPIDocManager.UI.Shared.Api;
using WAPIDocManager.UI.Shared.Authentication;

namespace WAPIDocManager.UI.Features.Auth.Services;

/// L'endpoint di accesso è anonimo: a differenza di tutti gli altri, questo client HTTP non monta il gestore che
/// allega il token alle richieste.
public class AuthApiService : IAuthService
{
    /// Percorso relativo, senza slash iniziale, così da combinarsi con l'indirizzo base del client HTTP.
    public const string LoginPath = "api/v1/auth/login";

    private readonly HttpClient _httpClient;
    private readonly IUserSessionStore _sessionStore;

    public AuthApiService(
        HttpClient httpClient,
        IUserSessionStore sessionStore)
    {
        _httpClient = httpClient;
        _sessionStore = sessionStore;
    }

    /// <inheritdoc />
    public async Task<UserSession> LoginAsync(LoginModel model, CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
            LoginPath,
            (LoginRequest)model,
            JsonDefaults.Options,
            cancellationToken);

        // credenziali errate: il server risponde 401, che la lettura della risposta trasforma in eccezione
        LoginResponse login = await ApiResponseReader.ReadAsync<LoginResponse>(response, cancellationToken);

        UserSession session = (UserSession)login;

        // il salvataggio solleva il cambio di sessione, che aggiorna lo stato di autenticazione dell'interfaccia
        await _sessionStore.SetAsync(session);

        return session;
    }

    /// <inheritdoc />
    public async Task LogoutAsync()
    {
        await _sessionStore.ClearAsync();
    }
}
