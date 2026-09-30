using FluentValidation;
using WAPIDocManager.UI.Features.Users.Models;
using WAPIDocManager.UI.Shared.Validation;

namespace WAPIDocManager.UI.Features.Users.Validators;

/// Regole del form di registrazione, allineate a quelle applicate dal servizio su POST api/v1/users/register.
/// I ruoli non hanno regole: anche nessun ruolo è una scelta legittima.
/// I messaggi non sono testi ma chiavi di risorsa, tradotte nel momento in cui vengono mostrate.
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
