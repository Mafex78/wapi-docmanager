using WAPIDocManager.UI.Features.Auth.Models;

namespace WAPIDocManager.UI.Features.Auth.Services.Dto;

/// Body di POST api/v1/auth/login.
public record LoginRequest
{
    public string? Email { get; init; }
    public string? Password { get; init; }

    /// Dal form al body: email ripulita dagli spazi, password invariata.
    public static explicit operator LoginRequest(LoginModel model)
    {
        return new LoginRequest
        {
            Email = model.Email?.Trim(),
            Password = model.Password
        };
    }
}
