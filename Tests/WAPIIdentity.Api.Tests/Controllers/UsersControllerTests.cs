using Microsoft.AspNetCore.Mvc;
using Moq;
using Shared.Domain.Types;
using WAPIIdentity.Application.Dto;
using WAPIIdentity.Application.Services;
using WAPIIdentity.Controllers;
using Xunit;

namespace WAPIIdentity.Api.Tests.Controllers;

public class UsersControllerTests
{
    [Fact]
    public async Task RegisterAsync_Returns_Ok_With_Response()
    {
        var expected = new RegisterUserResponse { Id = "u1" };

        var service = new Mock<IUserService>();
        service
            .Setup(s => s.RegisterAsync(It.IsAny<RegisterUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var sut = new UsersController(service.Object);

        var result = await sut.RegisterAsync(
            new RegisterUserRequest
            {
                Email = "a@b.com",
                Password = "pwd",
                Roles = new List<RoleType> { RoleType.Admin }
            },
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task RegisterAsync_Propagates_Exceptions()
    {
        var service = new Mock<IUserService>();
        service
            .Setup(s => s.RegisterAsync(It.IsAny<RegisterUserRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("invalid"));

        var sut = new UsersController(service.Object);

        await Assert.ThrowsAsync<ArgumentException>(
            () => sut.RegisterAsync(new RegisterUserRequest(), CancellationToken.None));
    }
}
