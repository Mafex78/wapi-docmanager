using FluentValidation.TestHelper;
using WAPIDocManager.UI.Features.Auth.Models;
using WAPIDocManager.UI.Features.Auth.Validators;
using WAPIDocManager.UI.Shared.Validation;

namespace WAPIDocManager.UI.Tests.Features.Auth;

/// <summary>
/// Regole del form di login. I messaggi attesi sono le CHIAVI di ValidationKeys: il testo viene tradotto
/// alla resa da FieldValidationMessage, quindi è la chiave a dover essere corretta.
/// </summary>
public class LoginModelValidatorTests
{
    private readonly LoginModelValidator _validator = new();

    [Fact]
    public void Empty_Model_Requires_Email_And_Password()
    {
        TestValidationResult<LoginModel> result = _validator.TestValidate(new LoginModel());

        result.ShouldHaveValidationErrorFor(model => model.Email).WithErrorMessage(ValidationKeys.Required);
        result.ShouldHaveValidationErrorFor(model => model.Password).WithErrorMessage(ValidationKeys.Required);
    }

    [Theory]
    [InlineData("non-una-email")]
    [InlineData("manca@")]
    public void Malformed_Email_Is_Rejected(string email)
    {
        TestValidationResult<LoginModel> result = _validator.TestValidate(new LoginModel { Email = email, Password = "x" });

        result.ShouldHaveValidationErrorFor(model => model.Email).WithErrorMessage(ValidationKeys.EmailInvalid);
    }

    [Fact]
    public void Valid_Credentials_Have_No_Errors()
    {
        TestValidationResult<LoginModel> result = _validator.TestValidate(new LoginModel { Email = "admin@wapi.it", Password = "secret" });

        result.ShouldNotHaveAnyValidationErrors();
    }
}
