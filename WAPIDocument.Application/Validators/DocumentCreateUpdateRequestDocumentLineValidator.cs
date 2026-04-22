using FluentValidation;
using WAPIDocument.Application.Dto.Document;

namespace WAPIDocument.Application.Validators;

public class DocumentCreateUpdateRequestDocumentLineValidator : AbstractValidator<DocumentCreateUpdateRequestDocumentLine>
{
    public DocumentCreateUpdateRequestDocumentLineValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity greater than 0")
            .ScalePrecision(2, 9)
            .WithMessage("Scale precision for Quantity is 9,2");
        
        RuleFor(x => x.UnitPrice)
            .GreaterThan(0)
            .WithMessage("UnitPrice greater than 0")
            .ScalePrecision(2, 12)
            .WithMessage("Scale precision for UnitPrice is 12,2");
    }
}