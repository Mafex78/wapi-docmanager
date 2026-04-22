using FluentValidation;
using Shared.Domain;
using WAPIIdentity.Application.Dto;
using WAPIIdentity.Domain.Entities;
using WAPIIdentity.Domain.Repositories;

namespace WAPIIdentity.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<RegisterUserRequest> _registerUserRequestValidator;
    
    public UserService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IValidator<RegisterUserRequest> registerUserRequestValidator)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _registerUserRequestValidator = registerUserRequestValidator;
    }
    
    public async Task<RegisterUserResponse?> RegisterAsync(RegisterUserRequest model, CancellationToken ct = default)
    {
        await _registerUserRequestValidator.ValidateAndThrowAsync(
            model, 
            cancellationToken: ct);
        
        User user = (User)model;
        
        user.Id = Guid.NewGuid().ToString();
        user.CreatedAtUtc = DateTime.UtcNow;
        user.UpdatedAtUtc = DateTime.UtcNow;
        user.IsActive = true;
        
        await _userRepository.InsertAsync(user, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        return new RegisterUserResponse()
        {
            Id = user.Id,
        };
    }
}