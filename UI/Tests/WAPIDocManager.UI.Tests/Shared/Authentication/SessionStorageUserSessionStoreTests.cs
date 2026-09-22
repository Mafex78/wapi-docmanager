using WAPIDocManager.UI.Shared.Authentication;
using WAPIDocManager.UI.Tests.TestSupport;

namespace WAPIDocManager.UI.Tests.Shared.Authentication;

/// <summary>
/// Persistenza della sessione nel sessionStorage (simulato da FakeJsRuntime): round trip JSON tra istanze, cache in memoria,
/// evento SessionChanged e valore corrotto.
/// </summary>
public class SessionStorageUserSessionStoreTests
{
    [Fact]
    public async Task SetAsync_Persists_Session_And_Raises_Event()
    {
        var jsRuntime = new FakeJsRuntime();
        var store = new SessionStorageUserSessionStore(jsRuntime);
        UserSession? notified = null;
        store.SessionChanged += session => notified = session;
        UserSession session = CreateSession();

        await store.SetAsync(session);

        Assert.Same(session, notified);
        Assert.True(jsRuntime.SessionStorage.ContainsKey(SessionStorageUserSessionStore.StorageKey));
    }

    [Fact]
    public async Task GetAsync_Reads_Session_Saved_By_Previous_Instance()
    {
        var jsRuntime = new FakeJsRuntime();
        await new SessionStorageUserSessionStore(jsRuntime).SetAsync(CreateSession());

        UserSession? restored = await new SessionStorageUserSessionStore(jsRuntime).GetAsync();

        Assert.NotNull(restored);
        Assert.Equal("u1", restored.UserId);
        Assert.Equal("token-value", restored.Token);
        Assert.Equal(new DateTime(2026, 9, 14, 10, 0, 0, DateTimeKind.Utc), restored.ExpirationUtc);
        Assert.Equal(new[] { RoleType.Editor, RoleType.Viewer }, restored.Roles);
    }

    [Fact]
    public async Task GetAsync_Caches_Session_In_Memory()
    {
        var jsRuntime = new FakeJsRuntime();
        var store = new SessionStorageUserSessionStore(jsRuntime);

        await store.GetAsync();
        await store.GetAsync();

        Assert.Equal(1, jsRuntime.GetItemCalls);
    }

    [Fact]
    public async Task ClearAsync_Removes_Session_And_Raises_Null()
    {
        var jsRuntime = new FakeJsRuntime();
        var store = new SessionStorageUserSessionStore(jsRuntime);
        await store.SetAsync(CreateSession());
        bool raised = false;
        store.SessionChanged += session => raised = session is null;

        await store.ClearAsync();

        Assert.True(raised);
        Assert.Null(await store.GetAsync());
        Assert.Empty(jsRuntime.SessionStorage);
    }

    [Fact]
    public async Task GetAsync_Corrupted_Value_Returns_Null()
    {
        var jsRuntime = new FakeJsRuntime();
        jsRuntime.SessionStorage[SessionStorageUserSessionStore.StorageKey] = "{ corrupted";

        Assert.Null(await new SessionStorageUserSessionStore(jsRuntime).GetAsync());
    }

    private static UserSession CreateSession()
    {
        return new UserSession
        {
            UserId = "u1",
            Email = "editor@wapi.it",
            Token = "token-value",
            ExpirationUtc = new DateTime(2026, 9, 14, 10, 0, 0, DateTimeKind.Utc),
            Roles = new List<RoleType> { RoleType.Editor, RoleType.Viewer }
        };
    }
}
