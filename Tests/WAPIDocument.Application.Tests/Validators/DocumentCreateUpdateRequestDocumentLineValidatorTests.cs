using WAPIDocument.Application.Dto.Document;
using WAPIDocument.Application.Validators;
using Xunit;

namespace WAPIDocument.Application.Tests.Validators;

public class DocumentCreateUpdateRequestDocumentLineValidatorTests
{
    private readonly DocumentCreateUpdateRequestDocumentLineValidator _validator = new();

    [Fact]
    public void DocumentLineDto_Valid()
    {
        var model = new DocumentCreateUpdateRequestDocumentLine() { Description = "d", Quantity = 1.00M, UnitPrice = 1.00M };
        var result = _validator.Validate(model);
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void DocumentLineDto_Invalid_DescriptionNullOrEmpty(string? description)
    {
        var model = new DocumentCreateUpdateRequestDocumentLine { Description = description, Quantity = 1.00M, UnitPrice = 1.00M };
        var result = _validator.Validate(model);
        Assert.Contains(result.Errors, e => e.PropertyName == "Description");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DocumentLineDto_Invalid_QuantityNotGreaterThanZero(int qty)
    {
        var model = new DocumentCreateUpdateRequestDocumentLine { Description = "d", Quantity = qty, UnitPrice = 1.00M };
        var result = _validator.Validate(model);
        Assert.Contains(result.Errors, e => e.PropertyName == "Quantity");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DocumentLineDto_Invalid_UnitPriceNotGreaterThanZero(int price)
    {
        var model = new DocumentCreateUpdateRequestDocumentLine { Description = "d", Quantity = 1.00M, UnitPrice = price };
        var result = _validator.Validate(model);
        Assert.Contains(result.Errors, e => e.PropertyName == "UnitPrice");
    }
}
