using System.ComponentModel.DataAnnotations;
using WAPIDocManager.UI.Features.Auth.Contracts;
using WAPIDocManager.UI.Shared.Validation;

namespace WAPIDocManager.UI.Features.Auth.Models;

/// <summary>
/// Form di login (pagina <c>Features/Auth/Views/Pages/Login.razor</c>).
/// </summary>
/// <remarks>
/// Regole allineate a <c>WAPIIdentity.Application/Validators/LoginRequestValidator.cs</c>.
/// Convertito nel body con l'operatore esplicito di <c>Features/Auth/Contracts/LoginRequest</c> (email con trim).
/// </remarks>
public class LoginModel
{
    [Required(ErrorMessage = ValidationKeys.Required)]
    [EmailAddress(ErrorMessage = ValidationKeys.EmailInvalid)]
    public string? Email { get; set; }

    [Required(ErrorMessage = ValidationKeys.Required)]
    public string? Password { get; set; }
}
