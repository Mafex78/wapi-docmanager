using FluentValidation;
using WAPIDocManager.UI.Features.Documents.Models;
using WAPIDocManager.UI.Shared.Validation;

namespace WAPIDocManager.UI.Features.Documents.Validators;

/// Regole del form dei filtri dell'elenco documenti.
/// L'unico vincolo è la dimensione della pagina, che il server rifiuta oltre il massimo consentito. Nell'interfaccia
/// il valore arriva da un elenco a scelta fissa, quindi questa regola è una rete di sicurezza per il giorno in cui
/// quel campo diventasse libero.
public sealed class DocumentFilterValidator : AbstractValidator<DocumentFilter>
{
    public DocumentFilterValidator()
    {
        RuleFor(filter => filter.PageSize)
            .InclusiveBetween(1, DocumentFilter.MaxPageSize).WithMessage(ValidationKeys.PageSizeRange);
    }
}
