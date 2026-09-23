using FluentValidation.TestHelper;
using WAPIDocManager.UI.Features.Users.Models;
using WAPIDocManager.UI.Features.Users.Validators;
using WAPIDocManager.UI.Shared.Authentication;
using WAPIDocManager.UI.Shared.Validation;

namespace WAPIDocManager.UI.Tests.Features.Users;

/// <summary>
/// Regole del form di registrazione utente: email e password obbligatorie, ruoli liberi (il backend accetta
/// anche una lista vuota).
/// </summary>
public class RegisterUserModelValidatorTests
{
    private readonly RegisterUserModelValidator _validator = new();

    [Fact]
    public void Empty_Model_Requires_Email_And_Password()
    {
        TestValidationResult<RegisterUserModel> result = _validator.TestValidate(new RegisterUserModel());

        result.ShouldHaveValidationErrorFor(model => model.Email).WithErrorMessage(ValidationKeys.Required);
        result.ShouldHaveValidationErrorFor(model => model.Password).WithErrorMessage(ValidationKeys.Required);
    }

    [Fact]
    public void Roles_Are_Not_Validated()
    {
        var model = new RegisterUserModel { Email = "editor@wapi.it", Password = "secret", Roles = { RoleType.Editor } };

        TestValidationResult<RegisterUserModel> result = _validator.TestValidate(model);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
