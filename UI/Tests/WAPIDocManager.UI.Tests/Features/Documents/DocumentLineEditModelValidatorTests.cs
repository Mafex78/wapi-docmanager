using System.Globalization;
using FluentValidation.TestHelper;
using WAPIDocManager.UI.Features.Documents.Models;
using WAPIDocManager.UI.Features.Documents.Validators;
using WAPIDocManager.UI.Shared.Validation;

namespace WAPIDocManager.UI.Tests.Features.Documents;

/// <summary>
/// Regole di una riga documento: descrizione obbligatoria, quantità e prezzo maggiori di zero e dentro i limiti
/// di precisione accettati dal server (9,2 per la quantità, 12,2 per il prezzo unitario).
/// </summary>
public class DocumentLineEditModelValidatorTests
{
    private readonly DocumentLineEditModelValidator _validator = new();

    [Fact]
    public void Empty_Line_Reports_Description_Quantity_And_Price()
    {
        var line = new DocumentLineEditModel { Description = null, Quantity = 0, UnitPrice = 0 };

        TestValidationResult<DocumentLineEditModel> result = _validator.TestValidate(line);

        result.ShouldHaveValidationErrorFor(l => l.Description).WithErrorMessage(ValidationKeys.Required);
        result.ShouldHaveValidationErrorFor(l => l.Quantity).WithErrorMessage(ValidationKeys.GreaterThanZero);
        result.ShouldHaveValidationErrorFor(l => l.UnitPrice).WithErrorMessage(ValidationKeys.GreaterThanZero);
    }

    [Theory]
    // valori ammessi: fino a 9 cifre totali e 2 decimali
    [InlineData("1234567.12", true)]
    [InlineData("1.10", true)]
    [InlineData("0.5", true)]
    // oltre la precisione o la scala
    [InlineData("12345678", false)]
    [InlineData("1.123", false)]
    public void Quantity_Precision_Is_9_Scale_2(string value, bool expectedValid)
    {
        decimal quantity = decimal.Parse(value, CultureInfo.InvariantCulture);
        var line = new DocumentLineEditModel { Description = "riga", Quantity = quantity, UnitPrice = 1M };

        TestValidationResult<DocumentLineEditModel> result = _validator.TestValidate(line);

        if (expectedValid)
        {
            result.ShouldNotHaveValidationErrorFor(l => l.Quantity);
        }
        else
        {
            result.ShouldHaveValidationErrorFor(l => l.Quantity).WithErrorMessage(ValidationKeys.QuantityPrecision);
        }
    }

    [Fact]
    public void Unit_Price_Precision_Is_12_Scale_2()
    {
        var line = new DocumentLineEditModel { Description = "riga", Quantity = 1M, UnitPrice = 1.123M };

        TestValidationResult<DocumentLineEditModel> result = _validator.TestValidate(line);

        result.ShouldHaveValidationErrorFor(l => l.UnitPrice).WithErrorMessage(ValidationKeys.UnitPricePrecision);
    }

    [Fact]
    public void Valid_Line_Has_No_Errors()
    {
        var line = new DocumentLineEditModel { Description = "Consulenza", Quantity = 2M, UnitPrice = 150.50M };

        _validator.TestValidate(line).ShouldNotHaveAnyValidationErrors();
    }
}
