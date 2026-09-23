using FluentValidation.TestHelper;
using WAPIDocManager.UI.Features.Documents.Models;
using WAPIDocManager.UI.Features.Documents.Validators;
using WAPIDocManager.UI.Shared.Validation;

namespace WAPIDocManager.UI.Tests.Features.Documents;

/// <summary>
/// Dimensione pagina dei filtri: nella UI arriva da una select con opzioni fisse, la regola è una rete di sicurezza
/// allineata al limite che applica il server.
/// </summary>
public class DocumentFilterValidatorTests
{
    private readonly DocumentFilterValidator _validator = new();

    [Theory]
    [InlineData(1, true)]
    [InlineData(DocumentFilter.MaxPageSize, true)]
    [InlineData(0, false)]
    [InlineData(DocumentFilter.MaxPageSize + 1, false)]
    public void Page_Size_Must_Stay_Within_The_Server_Limit(int pageSize, bool expectedValid)
    {
        TestValidationResult<DocumentFilter> result = _validator.TestValidate(new DocumentFilter { PageSize = pageSize });

        if (expectedValid)
        {
            result.ShouldNotHaveAnyValidationErrors();
        }
        else
        {
            result.ShouldHaveValidationErrorFor(filter => filter.PageSize).WithErrorMessage(ValidationKeys.PageSizeRange);
        }
    }
}
