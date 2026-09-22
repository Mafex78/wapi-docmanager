namespace WAPIDocManager.UI.Features.Users.Contracts;

/// <summary>
/// Risposta della registrazione (<c>WAPIIdentity.Application/Dto/RegisterUserResponse.cs</c>): Id dell'utente creato.
/// </summary>
public record RegisterUserResponse
{
    public string? Id { get; init; }
}
