using FluentValidation;
using WAPIDocManager.UI.Features.Auth.Models;
using WAPIDocManager.UI.Shared.Validation;

namespace WAPIDocManager.UI.Features.Auth.Validators;

/// Regole del form di accesso, allineate a quelle applicate dal servizio su POST api/v1/auth/login:
/// allentarle qui fa passare il form e poi fallire la chiamata.
/// I messaggi non sono testi ma CHIAVI di risorsa: la traduzione avviene nel momento in cui il messaggio viene
/// mostrato. Una chiave nuova va aggiunta in entrambi i file di risorse.
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
