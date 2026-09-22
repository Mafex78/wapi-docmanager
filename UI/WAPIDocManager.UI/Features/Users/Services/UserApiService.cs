using System.Net.Http.Json;
using WAPIDocManager.UI.Features.Users.Contracts;
using WAPIDocManager.UI.Features.Users.Models;
using WAPIDocManager.UI.Shared.Api;

namespace WAPIDocManager.UI.Features.Users.Services;

/// <summary>
/// Gestione utenti su WAPIIdentity (POST api/v1/users/register, solo Admin)
/// </summary>
/// <remarks>
/// Typed HttpClient con BaseAddress = <c>Api:IdentityBaseUrl</c> e BearerTokenHandler (endpoint protetto).
/// Controller di riferimento: <c>WAPIIdentity/Controllers/UsersController.cs</c>.
/// Conversione del form nell'operatore esplicito di <c>Contracts/Identity/RegisterUserRequest</c>.
/// </remarks>
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
