using WAPIDocManager.UI.Features.Documents.Entities;
using WAPIDocManager.UI.Features.Documents.Models;

namespace WAPIDocManager.UI.Tests.Features.Documents;

/// <summary>
/// Anteprima dei totali (arrotondamento come il backend) e precompilazione del form di modifica.
/// </summary>
public class DocumentEditModelTests
{
    [Fact]
    public void Total_Sums_Line_Totals_Rounded_To_Two_Decimals()
    {
        var model = new DocumentEditModel
        {
            Lines =
            {
                new DocumentLineEditModel { Description = "A", Quantity = 1.5M, UnitPrice = 2.333M },
                new DocumentLineEditModel { Description = "B", Quantity = 2, UnitPrice = 10 }
            }
        };

        Assert.Equal(3.50M, model.Lines[0].Total);
        Assert.Equal(23.50M, model.Total);
    }

    [Fact]
    public void ExplicitConversion_From_Document_Copies_Editable_Fields()
    {
        var document = new Document
        {
            Id = "1",
            Type = DocumentType.Proforma,
            Status = DocumentStatus.Ready,
            Date = new DateTime(2026, 9, 14, 0, 0, 0, DateTimeKind.Utc),
            Customer = new Customer { Name = "ACME", Email = "info@acme.it", VatNumber = "IT123", Address = "Via Roma 1" },
            Lines = new List<DocumentLine>
            {
                new() { Description = "Line", Quantity = 2, UnitPrice = 5, Total = 10 }
            }
        };

        DocumentEditModel model = (DocumentEditModel)document;

        Assert.Equal(DocumentType.Proforma, model.Type);
        Assert.Equal(new DateTime(2026, 9, 14, 0, 0, 0, DateTimeKind.Utc), model.Date);
        Assert.Equal("ACME", model.Customer.Name);
        Assert.Equal("info@acme.it", model.Customer.Email);
        Assert.Equal("IT123", model.Customer.VatNumber);
        Assert.Equal("Via Roma 1", model.Customer.Address);
        DocumentLineEditModel line = Assert.Single(model.Lines);
        Assert.Equal("Line", line.Description);
        Assert.Equal(2, line.Quantity);
        Assert.Equal(5, line.UnitPrice);
    }
}
