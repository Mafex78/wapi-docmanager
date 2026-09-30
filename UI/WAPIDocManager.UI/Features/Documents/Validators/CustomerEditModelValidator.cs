using FluentValidation;
using WAPIDocManager.UI.Features.Documents.Models;
using WAPIDocManager.UI.Shared.Validation;

namespace WAPIDocManager.UI.Features.Documents.Validators;

/// Regole dei dati del cliente dentro il form del documento.
/// Nessun campo è obbligatorio: l'unico controllo è che l'email, SE compilata, sia formalmente valida. Lasciarla
/// vuota è legittimo, quindi la regola vale solo a campo valorizzato.
public sealed class CustomerEditModelValidator : AbstractValidator<CustomerEditModel>
{
    public CustomerEditModelValidator()
    {
        RuleFor(customer => customer.Email)
            .EmailAddress().WithMessage(ValidationKeys.EmailInvalid)
            .When(customer => !string.IsNullOrWhiteSpace(customer.Email));
    }
}
