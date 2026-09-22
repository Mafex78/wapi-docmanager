using System.Globalization;
using WAPIDocManager.UI.Shared.Validation;

namespace WAPIDocManager.UI.Tests.Shared.Validation;

/// <summary>
/// Equivalenza con ScalePrecision di FluentValidation (precisione 9 e scala 2 = regola della quantità di riga).
/// I valori sono stringhe perché i decimal non sono ammessi negli attributi InlineData.
/// </summary>
public class DecimalPrecisionAttributeTests
{
    [Theory]
    [InlineData("1234567.12", true)]
    [InlineData("-1234567.99", true)]
    [InlineData("1.10", true)]
    [InlineData("0.5", true)]
    [InlineData("12345678", false)]
    [InlineData("1.123", false)]
    public void IsValid_Precision_9_Scale_2(string value, bool expected)
    {
        var attribute = new DecimalPrecisionAttribute(9, 2);

        Assert.Equal(expected, attribute.IsValid(decimal.Parse(value, CultureInfo.InvariantCulture)));
    }

    [Fact]
    public void IsValid_Null_Returns_True()
    {
        Assert.True(new DecimalPrecisionAttribute(9, 2).IsValid(null));
    }
}
