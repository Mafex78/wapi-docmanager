using WAPIDocManager.UI.Features.Users.Contracts;
using WAPIDocManager.UI.Shared.Authentication;
using WAPIDocManager.UI.Shared.Validation;

namespace WAPIDocManager.UI.Features.Users.Models;

/// <summary>
/// Form di registrazione utente (pagina <c>Features/Users/Views/Pages/UserRegister.razor</c>, solo Admin).
/// </summary>
/// <remarks>
/// Regole allineate a <c>WAPIIdentity.Application/Validators/RegisterUserRequestValidator.cs</c>.
/// L'unicità dell'email è verificata solo dal server.
/// </remarks>
public class RegisterUserModel
{
    public string? Email { get; set; }
    public string? Password { get; set; }

    /// <summary>
    /// Ruoli da assegnare; il backend accetta anche una lista vuota (duplicati rimossi dall'operatore esplicito di <c>Features/Users/Contracts/RegisterUserRequest</c>).
    /// </summary>
    public List<RoleType> Roles { get; set; } = new();
}
