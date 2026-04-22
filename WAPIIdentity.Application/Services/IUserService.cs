using WAPIIdentity.Application.Dto;

namespace WAPIIdentity.Application.Services;

public interface IUserService
{
    Task<RegisterUserResponse?> RegisterAsync(RegisterUserRequest model, CancellationToken ct = default);
}