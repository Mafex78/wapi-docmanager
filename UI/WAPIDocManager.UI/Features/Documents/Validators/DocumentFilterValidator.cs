using FluentValidation;
using WAPIDocManager.UI.Features.Documents.Models;
using WAPIDocManager.UI.Shared.Validation;

namespace WAPIDocManager.UI.Features.Documents.Validators;

/// <summary>
/// Regole del form dei filtri della lista documenti
/// </summary>
/// <remarks>
/// L'unico vincolo è la dimensione pagina, che il server rifiuta oltre <see cref="DocumentFilter.MaxPageSize"/>.
/// Nella UI il valore arriva da una select con opzioni fisse, quindi la regola è una rete di sicurezza:
/// prima della migrazione a FluentValidation l'attributo [Range] era presente ma inerte, perché quel form
/// non montava alcun componente di validazione.
/// </remarks>
public sealed class DocumentFilterValidator : AbstractValidator<DocumentFilter>
{
    public DocumentFilterValidator()
    {
        RuleFor(filter => filter.PageSize)
            .InclusiveBetween(1, DocumentFilter.MaxPageSize).WithMessage(ValidationKeys.PageSizeRange);
    }
}
