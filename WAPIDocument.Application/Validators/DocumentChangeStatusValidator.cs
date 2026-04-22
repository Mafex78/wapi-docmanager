using FluentValidation;
using WAPIDocument.Application.Dto;
using WAPIDocument.Domain.Entities.Documents;

namespace WAPIDocument.Application.Validators;

public class DocumentChangeStatusValidator : AbstractValidator<DocumentChangeStatusContext>
{
    public DocumentChangeStatusValidator()
    {
        RuleFor(x => x.NewStatus)
            .NotNull()
            .WithMessage("New status is required")
            .NotEqual(DocumentStatus.Draft)
            .WithErrorCode("Draft is invalid status")
            .IsInEnum()
            .WithMessage("New status is invalid");
    }
}