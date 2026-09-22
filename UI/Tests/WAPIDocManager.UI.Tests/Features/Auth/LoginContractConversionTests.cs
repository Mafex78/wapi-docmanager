using WAPIDocManager.UI.Features.Auth.Contracts;
using WAPIDocManager.UI.Shared.Authentication;
using WAPIDocManager.UI.Tests.TestSupport;

namespace WAPIDocManager.UI.Tests.Features.Auth;

/// <summary>
/// Casi limite dell'operatore esplicito LoginResponse → UserSession (data senza fuso, ruoli dal token).
/// Il caso normale è coperto da <see cref="AuthApiServiceTests"/>.
/// </summary>
public class LoginContractConversionTests
{
    [Fact]
    public void LoginResponse_Expiration_Without_Time_Zone_Is_Treated_As_Utc()
    {
        var response = new LoginResponse
        {
            UserId = "u1",
            Email = "viewer@wapi.it",
            Token = JwtTokenFactory.Create("""{ "role": "Viewer" }"""),
            TokenExpiration = new DateTime(2026, 9, 16, 10, 0, 0, DateTimeKind.Unspecified)
        };

        UserSession session = (UserSession)response;

        Assert.Equal(DateTimeKind.Utc, session.ExpirationUtc.Kind);
        Assert.Equal(new DateTime(2026, 9, 16, 10, 0, 0, DateTimeKind.Utc), session.ExpirationUtc);
        Assert.Equal(new[] { RoleType.Viewer }, session.Roles);
    }
}
