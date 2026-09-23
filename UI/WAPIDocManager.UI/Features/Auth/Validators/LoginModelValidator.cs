using FluentValidation;
using WAPIDocManager.UI.Features.Auth.Models;
using WAPIDocManager.UI.Shared.Validation;

namespace WAPIDocManager.UI.Features.Auth.Validators;

/// <summary>
/// Regole del form di login, allineate a <c>WAPIIdentity.Application/Validators/LoginRequestValidator.cs</c>
/// </summary>
/// <remarks>
/// I messaggi sono CHIAVI di <see cref="ValidationKeys"/>, non testi: la traduzione avviene alla resa in
/// <c>Shared/Forms/FieldValidationMessage</c>. Una chiave nuova va aggiunta in entrambi i file .resx.
/// Registrato nella DI da <c>AuthFeatureRegistration</c> e risolto dal componente &lt;FluentValidator /&gt;.
/// </remarks>
public sealed class LoginModelValidator : AbstractValidator<LoginModel>
{
    public LoginModelValidator()
    {
        RuleFor(model => model.Email)
            .NotEmpty().WithMessage(ValidationKeys.Required)
            .EmailAddress().WithMessage(ValidationKeys.EmailInvalid);

        RuleFor(model => model.Password)
            .NotEmpty().WithMessage(ValidationKeys.Required);
    }
}
