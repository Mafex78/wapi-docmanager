using WAPIIdentity.Application.Dto;
using WAPIIdentity.Application.Validators;
using Xunit;

namespace WAPIIdentity.Application.Tests.Validators;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public void LoginRequest_Valid()
    {
        var r = _validator.Validate(new LoginRequest { Email = "a@b.com", Password = "pwd" });
        Assert.True(r.IsValid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void LoginRequest_Invalid_EmailInvalid(string? email)
    {
        var r = _validator.Validate(new LoginRequest { Email = email, Password = "pwd" });
        Assert.Contains(r.Errors, e => e.PropertyName == "Email");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void LoginRequest_Invalid_PasswordInvalid(string? password)
    {
        var r = _validator.Validate(new LoginRequest { Email = "a@b.com", Password = password });
        Assert.Contains(r.Errors, e => e.PropertyName == "Password");
    }
}
