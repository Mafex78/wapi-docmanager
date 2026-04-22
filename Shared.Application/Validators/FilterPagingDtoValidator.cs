using FluentValidation;
using Shared.Application.Dto;

namespace Shared.Application.Validators;

public class FilterPagingDtoValidator : AbstractValidator<FilterPagingDto>
{
    public FilterPagingDtoValidator()
    {
        RuleFor(x => x.PageSize)
            .LessThanOrEqualTo(20)
            .WithMessage("PageSize must be less than or equal to 20.");
    }
}