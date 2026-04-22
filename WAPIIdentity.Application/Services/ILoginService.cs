using WAPIIdentity.Application.Dto;

namespace WAPIIdentity.Application.Services;

public interface ILoginService
{
    Task<LoginResponse?> LoginAsync(LoginRequest model, CancellationToken ct = default);
}