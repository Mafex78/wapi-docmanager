using System.Net.Http.Json;
using WAPIDocManager.UI.Features.Auth.Contracts;
using WAPIDocManager.UI.Features.Auth.Models;
using WAPIDocManager.UI.Shared.Api;
using WAPIDocManager.UI.Shared.Authentication;

namespace WAPIDocManager.UI.Features.Auth.Services;

/// <summary>
/// Autenticazione su WAPIIdentity (POST api/v1/auth/login)
/// </summary>
/// <remarks>
/// Typed HttpClient con BaseAddress = <c>Api:IdentityBaseUrl</c> e SENZA BearerTokenHandler (endpoint anonimo).
/// Controller di riferimento: <c>WAPIIdentity/Controllers/AuthController.cs</c>.
/// Conversioni negli operatori espliciti di <c>Contracts/Identity/LoginRequest</c> e <c>LoginResponse</c>.
/// </remarks>
public class AuthApiService : IAuthService
{
    /// <summary>Percorso relativo (senza slash iniziale, così si combina con la BaseAddress).</summary>
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

        // credenziali errate: il server risponde 401 → ApiException
        LoginResponse login = await ApiResponseReader.ReadAsync<LoginResponse>(response, cancellationToken);

        UserSession session = (UserSession)login;

        // solleva SessionChanged → JwtAuthenticationStateProvider notifica la UI
        await _sessionStore.SetAsync(session);

        return session;
    }

    /// <inheritdoc />
    public async Task LogoutAsync()
    {
        await _sessionStore.ClearAsync();
    }
}
