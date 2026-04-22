using FluentValidation;
using Shared.Application.Validators;
using WAPIDocument.Application.Dto.Document;

namespace WAPIDocument.Application.Validators;

public class DocumentFindPagedByFilterRequestValidator : AbstractValidator<DocumentFindPagedByFilterRequest>
{
    public DocumentFindPagedByFilterRequestValidator()
    {
        RuleFor(x => x.DocumentTypes)
            .ForEach(x => x.IsInEnum()
                .WithMessage("Types is invalid"));
        
        RuleFor(x => x.DocumentStatuses)
            .ForEach(x => x.IsInEnum()
                .WithMessage("Statuses is invalid"));
        
        Include(new FilterPagingDtoValidator());
    }
}