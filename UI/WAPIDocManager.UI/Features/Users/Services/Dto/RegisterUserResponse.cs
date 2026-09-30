namespace WAPIDocManager.UI.Features.Users.Services.Dto;

/// Risposta di POST api/v1/users/register: l'identificativo dell'utente creato.
public record RegisterUserResponse
{
    public string? Id { get; init; }
}
