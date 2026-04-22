using WAPIDocument.Application.Dto.Document;
using WAPIDocument.Application.Validators;
using Xunit;

namespace WAPIDocument.Application.Tests.Validators;

public class DocumentUpdateRequestValidatorTests
{
    private readonly DocumentUpdateRequestValidator _validator = new();
    
    [Fact]
    public void DocumentUpdateRequest_NoLines_Valid()
    {
        var request = new DocumentUpdateRequest
        {
            Currency = "EUR",
            Date = DateTime.UtcNow,
        };

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }
}