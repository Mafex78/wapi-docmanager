using System.Net;
using System.Text.Json;
using Moq;
using WAPIDocManager.UI.Features.Auth.Models;
using WAPIDocManager.UI.Features.Auth.Services;
using WAPIDocManager.UI.Shared.Api;
using WAPIDocManager.UI.Shared.Authentication;
using WAPIDocManager.UI.Tests.TestSupport;

namespace WAPIDocManager.UI.Tests.Features.Auth;

/// <summary>
/// Login su WAPIIdentity: body inviato, sessione salvata con ruoli letti dal JWT e scadenza UTC, credenziali errate (401), logout.
/// </summary>
public class AuthApiServiceTests
{
    [Fact]
    public async Task LoginAsync_Success_Stores_Session_With_Roles_From_Token()
    {
        string token = JwtTokenFactory.Create("""{ "sub": "u1", "email": "admin@wapi.it", "role": [ "Admin", "Editor" ] }""");
        string responseJson = JsonSerializer.Serialize(new
        {
            userId = "u1",
            email = "admin@wapi.it",
            token,
            tokenExpiration = "2026-09-14T10:00:00Z"
        });
        FakeHttpMessageHandler handler = FakeHttpMessageHandler.Returning(HttpStatusCode.OK, responseJson);
        Mock<IUserSessionStore> store = CreateStore();
        var service = new AuthApiService(CreateClient(handler), store.Object);

        UserSession session = await service.LoginAsync(new LoginModel { Email = " admin@wapi.it ", Password = "secret" });

        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal("/api/v1/auth/login", handler.LastRequest.Uri.AbsolutePath);
        using (JsonDocument body = JsonDocument.Parse(handler.LastRequest.Body!))
        {
            Assert.Equal("admin@wapi.it", body.RootElement.GetProperty("email").GetString());
            Assert.Equal("secret", body.RootElement.GetProperty("password").GetString());
        }

        Assert.Equal("u1", session.UserId);
        Assert.Equal(token, session.Token);
        Assert.Equal(new DateTime(2026, 9, 14, 10, 0, 0, DateTimeKind.Utc), session.ExpirationUtc);
        Assert.Equal(DateTimeKind.Utc, session.ExpirationUtc.Kind);
        Assert.Equal(new[] { RoleType.Admin, RoleType.Editor }, session.Roles);
        store.Verify(s => s.SetAsync(session), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_Unauthorized_Throws_And_Does_Not_Store_Session()
    {
        FakeHttpMessageHandler handler = FakeHttpMessageHandler.Returning(
            HttpStatusCode.Unauthorized,
            """{ "type": "Unauthorized", "title": "Error", "status": 401, "detail": "" }""",
            "application/problem+json");
        Mock<IUserSessionStore> store = CreateStore();
        var service = new AuthApiService(CreateClient(handler), store.Object);

        ApiException exception = await Assert.ThrowsAsync<ApiException>(
            () => service.LoginAsync(new LoginModel { Email = "admin@wapi.it", Password = "wrong" }));

        Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
        store.Verify(s => s.SetAsync(It.IsAny<UserSession>()), Times.Never);
    }

    [Fact]
    public async Task LogoutAsync_Clears_Session()
    {
        Mock<IUserSessionStore> store = CreateStore();
        var service = new AuthApiService(CreateClient(FakeHttpMessageHandler.Returning(HttpStatusCode.OK)), store.Object);

        await service.LogoutAsync();

        store.Verify(s => s.ClearAsync(), Times.Once);
    }

    private static Mock<IUserSessionStore> CreateStore()
    {
        var store = new Mock<IUserSessionStore>();
        store.Setup(s => s.SetAsync(It.IsAny<UserSession>())).Returns(ValueTask.CompletedTask);
        store.Setup(s => s.ClearAsync()).Returns(ValueTask.CompletedTask);
        return store;
    }

    private static HttpClient CreateClient(FakeHttpMessageHandler handler)
    {
        return new HttpClient(handler) { BaseAddress = new Uri("https://localhost:7205/") };
    }
}
