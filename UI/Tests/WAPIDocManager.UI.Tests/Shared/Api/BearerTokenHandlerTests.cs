using System.Net;
using Moq;
using WAPIDocManager.UI.Shared.Api;
using WAPIDocManager.UI.Shared.Authentication;
using WAPIDocManager.UI.Tests.TestSupport;

namespace WAPIDocManager.UI.Tests.Shared.Api;

/// <summary>
/// Header Authorization e gestione della sessione: token valido, assente, scaduto (nessuna chiamata), risposta 401 (logout) e 403 (sessione mantenuta).
/// L'handler viene eseguito tramite HttpMessageInvoker con FakeHttpMessageHandler come InnerHandler.
/// </summary>
public class BearerTokenHandlerTests
{
    private static readonly DateTime Now = new(2026, 9, 14, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task SendAsync_With_Valid_Session_Adds_Bearer_Token()
    {
        Mock<IUserSessionStore> store = CreateStore(CreateSession(Now.AddMinutes(30)));
        FakeHttpMessageHandler inner = FakeHttpMessageHandler.Returning(HttpStatusCode.OK);

        await SendAsync(store, inner);

        Assert.Equal("Bearer", inner.LastRequest.Authorization?.Scheme);
        Assert.Equal("token-value", inner.LastRequest.Authorization?.Parameter);
        store.Verify(s => s.ClearAsync(), Times.Never);
    }

    [Fact]
    public async Task SendAsync_Without_Session_Sends_Request_Without_Authorization()
    {
        Mock<IUserSessionStore> store = CreateStore(null);
        FakeHttpMessageHandler inner = FakeHttpMessageHandler.Returning(HttpStatusCode.OK);

        await SendAsync(store, inner);

        Assert.Null(inner.LastRequest.Authorization);
    }

    [Fact]
    public async Task SendAsync_With_Expired_Session_Clears_Session_Without_Calling_Api()
    {
        Mock<IUserSessionStore> store = CreateStore(CreateSession(Now.AddMinutes(-1)));
        FakeHttpMessageHandler inner = FakeHttpMessageHandler.Returning(HttpStatusCode.OK);

        ApiException exception = await Assert.ThrowsAsync<ApiException>(() => SendAsync(store, inner));

        Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
        Assert.Empty(inner.Requests);
        store.Verify(s => s.ClearAsync(), Times.Once);
    }

    [Fact]
    public async Task SendAsync_Unauthorized_Response_Clears_Session()
    {
        Mock<IUserSessionStore> store = CreateStore(CreateSession(Now.AddMinutes(30)));
        FakeHttpMessageHandler inner = FakeHttpMessageHandler.Returning(HttpStatusCode.Unauthorized);

        using HttpResponseMessage response = await SendAsync(store, inner);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        store.Verify(s => s.ClearAsync(), Times.Once);
    }

    [Fact]
    public async Task SendAsync_Forbidden_Response_Keeps_Session()
    {
        Mock<IUserSessionStore> store = CreateStore(CreateSession(Now.AddMinutes(30)));
        FakeHttpMessageHandler inner = FakeHttpMessageHandler.Returning(HttpStatusCode.Forbidden);

        using HttpResponseMessage response = await SendAsync(store, inner);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        store.Verify(s => s.ClearAsync(), Times.Never);
    }

    private static async Task<HttpResponseMessage> SendAsync(Mock<IUserSessionStore> store, FakeHttpMessageHandler inner)
    {
        var handler = new BearerTokenHandler(store.Object, new FixedTimeProvider(Now))
        {
            InnerHandler = inner
        };

        using var invoker = new HttpMessageInvoker(handler);

        return await invoker.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, "https://localhost:7273/api/v1/documents"),
            CancellationToken.None);
    }

    private static Mock<IUserSessionStore> CreateStore(UserSession? session)
    {
        var store = new Mock<IUserSessionStore>();
        store.Setup(s => s.GetAsync()).ReturnsAsync(session);
        store.Setup(s => s.ClearAsync()).Returns(ValueTask.CompletedTask);
        return store;
    }

    private static UserSession CreateSession(DateTime expirationUtc)
    {
        return new UserSession
        {
            UserId = "u1",
            Email = "editor@wapi.it",
            Token = "token-value",
            ExpirationUtc = expirationUtc,
            Roles = new List<RoleType> { RoleType.Editor }
        };
    }
}
