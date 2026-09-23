using FluentValidation;
using WAPIDocManager.UI.Features.Documents.Models;
using WAPIDocManager.UI.Shared.Validation;

namespace WAPIDocManager.UI.Features.Documents.Validators;

/// <summary>
/// Regole di una riga documento, allineate a
/// <c>WAPIDocument.Application/Validators/DocumentCreateUpdateRequestDocumentLineValidator.cs</c>
/// </summary>
/// <remarks>
/// <see cref="IRuleBuilderOptions{T,TProperty}"/> <c>PrecisionScale</c> sostituisce l'attributo DecimalPrecision
/// scritto a mano: precisione totale e scala sono gli stessi vincoli che il server applica sul decimale.
/// </remarks>
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
