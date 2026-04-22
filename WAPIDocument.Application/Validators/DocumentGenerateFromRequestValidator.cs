using FluentValidation;
using WAPIDocument.Application.Dto.Document;

namespace WAPIDocument.Application.Validators;

public class DocumentGenerateFromRequestValidator : AbstractValidator<DocumentGenerateFromRequest>
{
    public DocumentGenerateFromRequestValidator()
    {
        RuleFor(x => x.DocumentType)
            .NotNull()
            .WithMessage("Document type is required")
            .IsInEnum()
            .WithMessage("New status is invalid");
    }
}