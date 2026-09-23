using FluentValidation;
using WAPIDocManager.UI.Features.Documents.Models;
using WAPIDocManager.UI.Shared.Validation;

namespace WAPIDocManager.UI.Features.Documents.Validators;

/// <summary>
/// Regole dei dati cliente dentro il form documento
/// </summary>
/// <remarks>
/// Il cliente non ha campi obbligatori sul client (il server valida i propri vincoli): qui si controlla solo che
/// l'email, SE valorizzata, sia formalmente valida — per questo la regola è condizionata con <c>When</c>,
/// che con gli attributi DataAnnotations avrebbe richiesto un attributo dedicato.
/// </remarks>
public sealed class CustomerEditModelValidator : AbstractValidator<CustomerEditModel>
{
    public CustomerEditModelValidator()
    {
        RuleFor(customer => customer.Email)
            .EmailAddress().WithMessage(ValidationKeys.EmailInvalid)
            .When(customer => !string.IsNullOrWhiteSpace(customer.Email));
    }
}
