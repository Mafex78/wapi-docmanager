using Microsoft.AspNetCore.Mvc;
using Moq;
using WAPIIdentity.Application.Dto;
using WAPIIdentity.Application.Services;
using WAPIIdentity.Controllers;
using Xunit;

namespace WAPIIdentity.Api.Tests.Controllers;

public class AuthControllerTests
{
    [Fact]
    public async Task LoginAsync_Returns_Ok_With_LoginResponse()
    {
        var expected = new LoginResponse
        {
            UserId = "u",
            Email = "e",
            Token = "T"
        };

        var service = new Mock<ILoginService>();
        service
            .Setup(s => s.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var sut = new AuthController(service.Object);

        var result = await sut.LoginAsync(new LoginRequest { Email = "e", Password = "p" }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task LoginAsync_Propagates_Unauthorized()
    {
        var service = new Mock<ILoginService>();
        service
            .Setup(s => s.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        var sut = new AuthController(service.Object);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => sut.LoginAsync(new LoginRequest(), CancellationToken.None));
    }
}
