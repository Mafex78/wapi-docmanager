using FluentValidation;
using WAPIIdentity.Application.Dto;
using WAPIIdentity.Domain.Entities;
using WAPIIdentity.Domain.Repositories;

namespace WAPIIdentity.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IValidator<RegisterUserRequest> _registerUserRequestValidator;
    
    public UserService(
        IUserRepository userRepository,
        IValidator<RegisterUserRequest> registerUserRequestValidator)
    {
        _userRepository = userRepository;
        _registerUserRequestValidator = registerUserRequestValidator;
    }
    
    public async Task<RegisterUserResponse?> RegisterAsync(RegisterUserRequest model, CancellationToken ct = default)
    {
        await _registerUserRequestValidator.ValidateAndThrowAsync(
            model, 
            cancellationToken: ct);
        
        User user = (User)model;

        user.Create();
        
        await _userRepository.InsertAsync(user, ct);
        
        return new RegisterUserResponse()
        {
            Id = user.Id,
        };
    }
}