using WAPIDocManager.UI.Features.Users.Models;
using WAPIDocManager.UI.Features.Users.Services.Dto;
using WAPIDocManager.UI.Shared.Authentication;

namespace WAPIDocManager.UI.Tests.Features.Users;

/// Casi limite dell'operatore esplicito RegisterUserModel → RegisterUserRequest (null e ruoli duplicati).
/// Il caso normale è coperto da UserApiServiceTests.
public class RegisterUserContractConversionTests
{
    [Fact]
    public void RegisterUserRequest_From_Model_Handles_Nulls_And_Duplicate_Roles()
    {
        var model = new RegisterUserModel { Email = null, Password = null, Roles = { RoleType.Admin, RoleType.Admin } };

        RegisterUserRequest request = (RegisterUserRequest)model;

        Assert.Equal(string.Empty, request.Email);
        Assert.Equal(string.Empty, request.Password);
        Assert.Equal(new[] { RoleType.Admin }, request.Roles);
    }
}
