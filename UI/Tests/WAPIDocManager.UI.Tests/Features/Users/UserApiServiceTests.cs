using System.Net;
using System.Text.Json;
using WAPIDocManager.UI.Features.Users.Models;
using WAPIDocManager.UI.Features.Users.Services;
using WAPIDocManager.UI.Shared.Api;
using WAPIDocManager.UI.Shared.Authentication;
using WAPIDocManager.UI.Tests.TestSupport;

namespace WAPIDocManager.UI.Tests.Features.Users;

/// <summary>
/// Registrazione utente: body con email normalizzata e ruoli numerici senza duplicati, errore di email già esistente.
/// </summary>
public class UserApiServiceTests
{
    [Fact]
    public async Task RegisterAsync_Posts_User_With_Numeric_Roles_And_Returns_Id()
    {
        FakeHttpMessageHandler handler = FakeHttpMessageHandler.Returning(HttpStatusCode.OK, """{ "id": "new-user" }""");
        var service = new UserApiService(CreateClient(handler));
        var model = new RegisterUserModel
        {
            Email = " viewer@wapi.it ",
            Password = "secret",
            Roles = { RoleType.Viewer, RoleType.Editor, RoleType.Viewer }
        };

        string? id = await service.RegisterAsync(model);

        Assert.Equal("new-user", id);
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal("/api/v1/users/register", handler.LastRequest.Uri.AbsolutePath);

        using JsonDocument body = JsonDocument.Parse(handler.LastRequest.Body!);
        Assert.Equal("viewer@wapi.it", body.RootElement.GetProperty("email").GetString());
        Assert.Equal("secret", body.RootElement.GetProperty("password").GetString());
        Assert.Equal(new[] { 2, 1 }, body.RootElement.GetProperty("roles").EnumerateArray().Select(role => role.GetInt32()));
    }

    [Fact]
    public async Task RegisterAsync_Existing_Email_Throws_With_Detail()
    {
        FakeHttpMessageHandler handler = FakeHttpMessageHandler.Returning(
            HttpStatusCode.BadRequest,
            """{ "title": "Error", "status": 400, "detail": "Validation failed: -- Email: Email already exists" }""",
            "application/problem+json");
        var service = new UserApiService(CreateClient(handler));

        ApiException exception = await Assert.ThrowsAsync<ApiException>(
            () => service.RegisterAsync(new RegisterUserModel { Email = "admin@wapi.it", Password = "secret" }));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        Assert.Contains("Email already exists", exception.Detail);
    }

    private static HttpClient CreateClient(FakeHttpMessageHandler handler)
    {
        return new HttpClient(handler) { BaseAddress = new Uri("https://localhost:7205/") };
    }
}
