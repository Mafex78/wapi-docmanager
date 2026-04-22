using FluentValidation;
using WAPIDocument.Application.Dto.Document;

namespace WAPIDocument.Application.Validators;

public class DocumentUpdateRequestValidator : AbstractValidator<DocumentUpdateRequest>
{
    public DocumentUpdateRequestValidator()
    {
        RuleFor(x => x.Date)
            .NotEmpty()
            .WithMessage("Date can't be empty");
        
        RuleForEach(x => x.DocumentLines)
            .SetValidator(new DocumentCreateUpdateRequestDocumentLineValidator());
    }
}