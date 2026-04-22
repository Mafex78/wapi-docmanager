using System.Linq.Expressions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using WAPIIdentity.Application.Dto;
using WAPIIdentity.Application.Services;
using WAPIIdentity.Domain.Entities;
using WAPIIdentity.Domain.Repositories;
using Xunit;

namespace WAPIIdentity.Application.Tests.Services;

public class LoginServiceTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IJwtTokenService> _jwtTokenService = new();
    private readonly Mock<ILogger<LoginService>> _logger = new();
    private readonly Mock<IValidator<LoginRequest>> _loginRequestValidator = new();

    private LoginService CreateService() => new(
        _userRepository.Object,
        _jwtTokenService.Object,
        _logger.Object,
        _loginRequestValidator.Object);

    [Fact]
    public async Task LoginService_LoginAsync_Ok()
    {
        var user = new User
        {
            Id = "u1",
            Email = "mail@x.com",
            Password = "pwd",
            IsActive = true
        };

        _userRepository
            .Setup(r => r.GetByFilterAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var expiration = DateTime.UtcNow.AddHours(1);
        _jwtTokenService
            .Setup(j => j.Generate(user))
            .Returns(new TokenResponse { Token = "TKN", Expiration = expiration });
        
        _loginRequestValidator
            .Setup(x => x.ValidateAsync(
                It.IsAny<ValidationContext>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        var service = CreateService();
        var result = await service.LoginAsync(new LoginRequest
        {
            Email = "mail@x.com",
            Password = "pwd"
        });

        Assert.NotNull(result);
        Assert.Equal("u1", result!.UserId);
        Assert.Equal("mail@x.com", result.Email);
        Assert.Equal("TKN", result.Token);
        Assert.Equal(expiration, result.TokenExpiration);
    }

    [Fact]
    public async Task LoginService_LoginAsync_Ko_Unauthorized()
    {
        _userRepository
            .Setup(r => r.GetByFilterAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _loginRequestValidator
            .Setup(x => x.ValidateAsync(
                It.IsAny<ValidationContext>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
                

        var service = CreateService();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.LoginAsync(new LoginRequest { Email = "x@test.com", Password = "y" }));
    }
}
