using WAPIDocument.Application.Dto.Document;
using WAPIDocument.Application.Validators;
using Xunit;

namespace WAPIDocument.Application.Tests.Validators;

public class DocumentCreateRequestValidatorTests
{
    private readonly DocumentCreateRequestValidator _validator = new();

    [Fact]
    public void DocumentCreateRequest_Valid()
    {
        var request = new DocumentCreateRequest
        {
            Currency = "EUR",
            DocumentLines = new List<DocumentCreateUpdateRequestDocumentLine>
            {
                new() { Description = "d", Quantity = 1.00M, UnitPrice = 1.00M }
            }
        };

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }
    
    [Fact]
    public void DocumentCreateRequest_NoLines_Valid()
    {
        var request = new DocumentCreateRequest
        {
            Currency = "EUR",
        };

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void DocumentCreateRequest_Invalid_ErrorInDocumentLines()
    {
        var request = new DocumentCreateRequest
        {
            Currency = "EUR",
            DocumentLines = new List<DocumentCreateUpdateRequestDocumentLine>
            {
                new() { Description = "", Quantity = 0, UnitPrice = 0 }
            }
        };
    
        var result = _validator.Validate(request);
    
        Assert.False(result.IsValid);
    }
}
