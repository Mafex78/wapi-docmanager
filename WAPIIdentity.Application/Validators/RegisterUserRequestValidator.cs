using FluentValidation;
using WAPIIdentity.Application.Dto;
using WAPIIdentity.Domain.Entities;
using WAPIIdentity.Domain.Repositories;

namespace WAPIIdentity.Application.Validators;

public class RegisterUserRequestValidator : AbstractValidator<RegisterUserRequest>
{
    public RegisterUserRequestValidator(
        IUserRepository userRepository)
    {
        RuleFor(x =>  x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email format is invalid")
            .MustAsync(async (email, ct) =>
             {
                 bool exists = false;
                 
                 User? user = await userRepository.GetByEmailAsync(email, ct);

                 exists = user is not null;
                 
                 return !exists;
             })
             .WithMessage("Email already exists");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required");

        RuleFor(x => x.Roles)
            .NotNull().WithErrorCode("Roles are required")
            .ForEach(x => x.IsInEnum()
                .WithMessage("Role is invalid"));
    }
}