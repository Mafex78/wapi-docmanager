using WAPIDocManager.UI.Features.Auth.Services;
using WAPIDocManager.UI.Shared.Authentication;
using WAPIDocManager.UI.Tests.TestSupport;

namespace WAPIDocManager.UI.Tests.Features.Auth;

/// Lettura dei ruoli dal payload del token nei due formati che il server può produrre, stringa singola o elenco,
/// nome lungo del claim, ruoli sconosciuti e token malformati.
public class JwtPayloadReaderTests
{
    [Fact]
    public void ReadRoles_Single_Role_Claim()
    {
        string token = JwtTokenFactory.Create("""{ "sub": "u1", "role": "Viewer" }""");

        Assert.Equal(new[] { RoleType.Viewer }, JwtPayloadReader.ReadRoles(token));
    }

    [Fact]
    public void ReadRoles_Array_Of_Roles()
    {
        string token = JwtTokenFactory.Create("""{ "role": [ "Admin", "Editor", "Admin" ] }""");

        Assert.Equal(new[] { RoleType.Admin, RoleType.Editor }, JwtPayloadReader.ReadRoles(token));
    }

    [Fact]
    public void ReadRoles_Long_Claim_Name()
    {
        string token = JwtTokenFactory.Create("""{ "http://schemas.microsoft.com/ws/2008/06/identity/claims/role": "Editor" }""");

        Assert.Equal(new[] { RoleType.Editor }, JwtPayloadReader.ReadRoles(token));
    }

    [Fact]
    public void ReadRoles_Ignores_Unknown_Roles()
    {
        string token = JwtTokenFactory.Create("""{ "role": [ "SuperUser", "99", "Viewer" ] }""");

        Assert.Equal(new[] { RoleType.Viewer }, JwtPayloadReader.ReadRoles(token));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-jwt")]
    [InlineData("header.%%%.signature")]
    public void ReadRoles_Malformed_Token_Returns_Empty(string token)
    {
        Assert.Empty(JwtPayloadReader.ReadRoles(token));
    }
}
