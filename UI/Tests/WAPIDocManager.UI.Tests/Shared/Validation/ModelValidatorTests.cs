using WAPIDocManager.UI.Features.Auth.Models;
using WAPIDocManager.UI.Features.Documents.Models;
using WAPIDocManager.UI.Shared.Validation;

namespace WAPIDocManager.UI.Tests.Shared.Validation;

/// <summary>
/// Validazione del grafo dei form model: righe annidate (istanza e proprietà corrette), chiavi dei messaggi, limite PageSize.
/// </summary>
public class ModelValidatorTests
{
    [Fact]
    public void Validate_Valid_Document_Returns_No_Failures()
    {
        var model = new DocumentEditModel
        {
            Lines = { new DocumentLineEditModel { Description = "Line", Quantity = 2, UnitPrice = 10.50M } }
        };

        Assert.Empty(ModelValidator.Validate(model));
    }

    [Fact]
    public void Validate_Document_Validates_Each_Line()
    {
        var validLine = new DocumentLineEditModel { Description = "Line", Quantity = 1, UnitPrice = 1 };
        var invalidLine = new DocumentLineEditModel { Description = " ", Quantity = 0, UnitPrice = 1.123M };
        var model = new DocumentEditModel { Lines = { validLine, invalidLine } };

        IReadOnlyList<ValidationFailure> failures = ModelValidator.Validate(model);

        Assert.Equal(3, failures.Count);
        Assert.All(failures, failure => Assert.Same(invalidLine, failure.Instance));
        Assert.Contains(failures, f => f.MemberName == nameof(DocumentLineEditModel.Description) && f.MessageKey == ValidationKeys.Required);
        Assert.Contains(failures, f => f.MemberName == nameof(DocumentLineEditModel.Quantity) && f.MessageKey == ValidationKeys.GreaterThanZero);
        Assert.Contains(failures, f => f.MemberName == nameof(DocumentLineEditModel.UnitPrice) && f.MessageKey == ValidationKeys.UnitPricePrecision);
    }

    [Fact]
    public void Validate_Login_With_Invalid_Email_And_Missing_Password()
    {
        var model = new LoginModel { Email = "not-an-email" };

        IReadOnlyList<ValidationFailure> failures = ModelValidator.Validate(model);

        Assert.Contains(failures, f => f.MemberName == nameof(LoginModel.Email) && f.MessageKey == ValidationKeys.EmailInvalid);
        Assert.Contains(failures, f => f.MemberName == nameof(LoginModel.Password) && f.MessageKey == ValidationKeys.Required);
    }

    [Theory]
    [InlineData(20, true)]
    [InlineData(21, false)]
    [InlineData(0, false)]
    public void Validate_Filter_PageSize_Limit(int pageSize, bool isValid)
    {
        var filter = new DocumentFilter { PageSize = pageSize };

        IReadOnlyList<ValidationFailure> failures = ModelValidator.Validate(filter);

        Assert.Equal(isValid, failures.Count == 0);
    }
}
