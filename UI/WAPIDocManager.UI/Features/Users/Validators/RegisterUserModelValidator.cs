using FluentValidation;
using WAPIDocManager.UI.Features.Users.Models;
using WAPIDocManager.UI.Shared.Validation;

namespace WAPIDocManager.UI.Features.Users.Validators;

/// <summary>
/// Regole del form di registrazione utente, allineate a
/// <c>WAPIIdentity.Application/Validators/RegisterUserRequestValidator.cs</c>
/// </summary>
/// <remarks>
/// I ruoli non hanno regole: il backend accetta anche una lista vuota.
/// Messaggi come chiavi di <see cref="ValidationKeys"/>, tradotte alla resa da <c>FieldValidationMessage</c>.
/// </remarks>
public sealed class RegisterUserModelValidator : AbstractValidator<RegisterUserModel>
{
    public RegisterUserModelValidator()
    {
        RuleFor(model => model.Email)
            .NotEmpty().WithMessage(ValidationKeys.Required)
            .EmailAddress().WithMessage(ValidationKeys.EmailInvalid);

        RuleFor(model => model.Password)
            .NotEmpty().WithMessage(ValidationKeys.Required);
    }
}
