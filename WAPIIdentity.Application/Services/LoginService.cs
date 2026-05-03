using FluentValidation;
using Microsoft.Extensions.Logging;
using WAPIIdentity.Application.Dto;
using WAPIIdentity.Domain.Entities;
using WAPIIdentity.Domain.Repositories;

namespace WAPIIdentity.Application.Services;

public class LoginService : ILoginService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<LoginService> _logger;
    private readonly IValidator<LoginRequest> _loginRequestValidator;

    public LoginService(
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService,
        ILogger<LoginService> logger,
        IValidator<LoginRequest> loginRequestValidator)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
        _loginRequestValidator = loginRequestValidator;
    }
    
    public async Task<LoginResponse?> LoginAsync(LoginRequest model, CancellationToken ct = default)
    {
        _logger.LogDebug("LoginAsync start ...");
        
        await _loginRequestValidator.ValidateAndThrowAsync(
            model, 
            cancellationToken: ct);
        
        User? user = await _userRepository.GetByFilterAsync(
            u => u.Email!.ToLower() == model.Email!.ToLower() &&
                 u.Password == model.Password &&
                 u.IsActive,
            ct);

        if (user is null)
        {
            throw new UnauthorizedAccessException();
        }
        
        // Generate Jwt Token
        TokenResponse tokenResponse = _jwtTokenService.Generate(user);

        _logger.LogDebug("Token generated for user with email {Email}", user.Email);
        return new LoginResponse()
        {
            UserId = user.Id,
            Email = user.Email,
            Token = tokenResponse.Token,
            TokenExpiration = tokenResponse.Expiration,
        };
    }
}