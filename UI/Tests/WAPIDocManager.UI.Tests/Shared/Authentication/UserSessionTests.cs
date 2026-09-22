using WAPIDocManager.UI.Shared.Authentication;

namespace WAPIDocManager.UI.Tests.Shared.Authentication;

/// <summary>
/// Scadenza (senza tolleranza, come le API) e ruoli della sessione utente.
/// </summary>
public class UserSessionTests
{
    private static readonly DateTime Expiration = new(2026, 9, 14, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void IsExpired_Before_Expiration_Returns_False()
    {
        var session = new UserSession { ExpirationUtc = Expiration };

        Assert.False(session.IsExpired(Expiration.AddSeconds(-1)));
    }

    [Fact]
    public void IsExpired_At_Expiration_Returns_True()
    {
        var session = new UserSession { ExpirationUtc = Expiration };

        Assert.True(session.IsExpired(Expiration));
    }

    [Fact]
    public void IsInRole_Checks_Roles()
    {
        var session = new UserSession { Roles = new List<RoleType> { RoleType.Editor } };

        Assert.True(session.IsInRole(RoleType.Editor));
        Assert.False(session.IsInRole(RoleType.Admin));
    }
}
