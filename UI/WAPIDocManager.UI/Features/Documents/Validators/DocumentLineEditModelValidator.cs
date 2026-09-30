using FluentValidation;
using WAPIDocManager.UI.Features.Documents.Models;
using WAPIDocManager.UI.Shared.Validation;

namespace WAPIDocManager.UI.Features.Documents.Validators;

/// Regole di una riga di documento, allineate a quelle applicate dal server.
/// I vincoli di precisione e scala sui decimali sono gli stessi che il server impone: allentarli qui farebbe
/// passare il form e poi fallire il salvataggio.
public sealed class DocumentLineEditModelValidator : AbstractValidator<DocumentLineEditModel>
{
    public DocumentLineEditModelValidator()
    {
        RuleFor(line => line.Description)
            .NotEmpty().WithMessage(ValidationKeys.Required);

        RuleFor(line => line.Quantity)
            .GreaterThan(0).WithMessage(ValidationKeys.GreaterThanZero)
            .PrecisionScale(9, 2, ignoreTrailingZeros: false).WithMessage(ValidationKeys.QuantityPrecision);

        RuleFor(line => line.UnitPrice)
            .GreaterThan(0).WithMessage(ValidationKeys.GreaterThanZero)
            .PrecisionScale(12, 2, ignoreTrailingZeros: false).WithMessage(ValidationKeys.UnitPricePrecision);
    }
}
