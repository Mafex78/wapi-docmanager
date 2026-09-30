using System.Net.Http.Json;
using WAPIDocManager.UI.Features.Users.Models;
using WAPIDocManager.UI.Features.Users.Services.Dto;
using WAPIDocManager.UI.Shared.Api;

namespace WAPIDocManager.UI.Features.Users.Services;

/// Chiamate di gestione degli utenti. L'endpoint è protetto, quindi questo client HTTP allega il token alle
/// richieste.
public class UserApiService : IUserService
{
    public const string RegisterPath = "api/v1/users/register";

    private readonly HttpClient _httpClient;

    public UserApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public async Task<string?> RegisterAsync(RegisterUserModel model, CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
            RegisterPath,
            (RegisterUserRequest)model,
            JsonDefaults.Options,
            cancellationToken);

        RegisterUserResponse result = await ApiResponseReader.ReadAsync<RegisterUserResponse>(response, cancellationToken);

        return result.Id;
    }
}
