using Moq;
using Shared.Domain.Types;
using WAPIIdentity.Application.Dto;
using WAPIIdentity.Application.Validators;
using WAPIIdentity.Domain.Entities;
using WAPIIdentity.Domain.Repositories;
using Xunit;

namespace WAPIIdentity.Application.Tests.Validators;

public class RegisterUserRequestValidatorTests
{
    private readonly Mock<IUserRepository> _userRepository;
    private RegisterUserRequestValidator _validator;

    public RegisterUserRequestValidatorTests()
    {
        _userRepository =  new Mock<IUserRepository>();
        _validator = new RegisterUserRequestValidator(_userRepository.Object);
    }

    [Fact]
    public async Task RegisterUserRequest_Valid()
    {
        _userRepository
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _validator.ValidateAsync(new RegisterUserRequest
        {
            Email = "a@b.com",
            Password = "pwd",
            Roles = new List<RoleType> { RoleType.Admin }
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task RegisterUserRequest_Invalid_EmptyEmail()
    {
        var result = await _validator.ValidateAsync(new RegisterUserRequest
        {
            Email = "",
            Password = "pwd",
            Roles = new List<RoleType>()
        });
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    [Fact]
    public async Task RegisterUserRequest_Invalid_EmailFormat()
    {
        var result = await _validator.ValidateAsync(new RegisterUserRequest
        {
            Email = "not-an-email",
            Password = "pwd",
            Roles = new List<RoleType>()
        });
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    [Fact]
    public async Task RegisterUserRequest_Invalid_EmailAlreadyExists()
    {
        _userRepository
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Email = "a@b.com" });
        
        _validator = new RegisterUserRequestValidator(_userRepository.Object);

        var result = await _validator.ValidateAsync(new RegisterUserRequest
        {
            Email = "a@b.com",
            Password = "pwd",
            Roles = new List<RoleType>()
        });

        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    [Fact]
    public async Task RegisterUserRequest_Invalid_EmptyPassword()
    {
        _userRepository
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _validator.ValidateAsync(new RegisterUserRequest
        {
            Email = "a@b.com",
            Password = "",
            Roles = new List<RoleType>()
        });
        Assert.Contains(result.Errors, e => e.PropertyName == "Password");
    }

    [Fact]
    public async Task RegisterUserRequest_Invalid_RoleInvalid()
    {
        _userRepository
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _validator.ValidateAsync(new RegisterUserRequest
        {
            Email = "a@b.com",
            Password = "pwd",
            Roles = new List<RoleType> { (RoleType)999 }
        });

        Assert.False(result.IsValid);
    }
}
