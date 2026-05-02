using FluentValidation;
using Moq;
using Shared.Domain.Types;
using WAPIIdentity.Application.Dto;
using WAPIIdentity.Application.Services;
using WAPIIdentity.Domain.Entities;
using WAPIIdentity.Domain.Repositories;
using Xunit;

namespace WAPIIdentity.Application.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IValidator<RegisterUserRequest>> _registerUserRequestValidator = new();

    private UserService CreateService() => new(
        _userRepository.Object, 
        _registerUserRequestValidator.Object);

    [Fact]
    public async Task UserService_RegisterAsync_Ok()
    {
        _userRepository
            .Setup(r => r.InsertAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _registerUserRequestValidator
            .Setup(x => x.ValidateAsync(
                It.IsAny<ValidationContext>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        var service = CreateService();
        var result = await service.RegisterAsync(new RegisterUserRequest
        {
            Email = "mail@x.com",
            Password = "pwd",
            Roles = new List<RoleType> { RoleType.Admin }
        });

        Assert.NotNull(result);
    }
}
