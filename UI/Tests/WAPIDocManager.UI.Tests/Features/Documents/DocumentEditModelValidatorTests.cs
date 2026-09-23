using FluentValidation.TestHelper;
using WAPIDocManager.UI.Features.Documents.Models;
using WAPIDocManager.UI.Features.Documents.Validators;
using WAPIDocManager.UI.Shared.Validation;

namespace WAPIDocManager.UI.Tests.Features.Documents;

/// <summary>
/// Regole del form documento: non ha vincoli propri, ma deve propagare la validazione al cliente annidato
/// e a OGNI riga (SetValidator e RuleForEach), che è ciò che prima faceva il motore di validazione scritto a mano.
/// </summary>
public class DocumentEditModelValidatorTests
{
    private readonly DocumentEditModelValidator _validator = new();

    [Fact]
    public void Invalid_Line_Is_Reported_With_Its_Index()
    {
        var model = new DocumentEditModel
        {
            Lines =
            {
                new DocumentLineEditModel { Description = "valida", Quantity = 1M, UnitPrice = 10M },
                new DocumentLineEditModel { Description = null, Quantity = 0M, UnitPrice = 1M }
            }
        };

        TestValidationResult<DocumentEditModel> result = _validator.TestValidate(model);

        // il percorso con l'indice è ciò che permette a FieldValidationMessage di mostrare l'errore sulla riga giusta
        result.ShouldHaveValidationErrorFor("Lines[1].Description").WithErrorMessage(ValidationKeys.Required);
        result.ShouldHaveValidationErrorFor("Lines[1].Quantity").WithErrorMessage(ValidationKeys.GreaterThanZero);
        result.ShouldNotHaveValidationErrorFor("Lines[0].Description");
    }

    [Fact]
    public void Customer_Email_Is_Validated_Only_When_Filled()
    {
        var withBadEmail = new DocumentEditModel { Customer = new CustomerEditModel { Email = "non-una-email" } };
        var withoutEmail = new DocumentEditModel { Customer = new CustomerEditModel { Email = "  " } };

        _validator.TestValidate(withBadEmail)
            .ShouldHaveValidationErrorFor("Customer.Email").WithErrorMessage(ValidationKeys.EmailInvalid);
        _validator.TestValidate(withoutEmail)
            .ShouldNotHaveValidationErrorFor("Customer.Email");
    }

    [Fact]
    public void Document_Without_Lines_Is_Valid_On_The_Client()
    {
        // il vincolo "almeno una riga" è del server: il client non lo duplica
        _validator.TestValidate(new DocumentEditModel()).ShouldNotHaveAnyValidationErrors();
    }
}
