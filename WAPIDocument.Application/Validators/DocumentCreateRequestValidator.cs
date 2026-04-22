using FluentValidation;
using WAPIDocument.Application.Dto.Document;

namespace WAPIDocument.Application.Validators;

public class DocumentCreateRequestValidator : AbstractValidator<DocumentCreateRequest>
{
    public DocumentCreateRequestValidator()
    {
        RuleForEach(x => x.DocumentLines)
            .SetValidator(new DocumentCreateUpdateRequestDocumentLineValidator());
    }
}