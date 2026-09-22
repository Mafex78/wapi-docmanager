using WAPIDocManager.UI.Features.Documents.Contracts;
using WAPIDocManager.UI.Features.Documents.Entities;
using WAPIDocManager.UI.Features.Documents.Models;

namespace WAPIDocManager.UI.Tests.Features.Documents;

/// <summary>
/// Casi limite degli operatori espliciti dei contratti dei documenti (valori null, stringhe vuote).
/// Il caso normale è coperto da <see cref="DocumentApiServiceTests"/>.
/// </summary>
public class DocumentContractConversionTests
{
    [Fact]
    public void DocumentResponse_With_Null_Customer_And_Collections_Converts_To_Empty_Values()
    {
        var response = new DocumentResponse { Id = "abc", Customer = null, DocumentLines = null, LinkedDocuments = null };

        Document document = (Document)response;

        Assert.Equal("abc", document.Id);
        Assert.NotNull(document.Customer);
        Assert.Null(document.Customer.Name);
        Assert.Empty(document.Lines);
        Assert.Empty(document.LinkedDocuments);
    }

    [Fact]
    public void DocumentAttachResponse_With_Null_Links_Converts_To_Empty_List()
    {
        List<DocumentLink> links = (List<DocumentLink>)new DocumentAttachResponse { LinkedDocuments = null };

        Assert.Empty(links);
    }

    [Fact]
    public void CustomerDto_From_EditModel_Trims_Values_And_Nulls_Empty_Ones()
    {
        var customer = new CustomerEditModel { Name = "  ACME  ", Email = "", VatNumber = "   ", Address = "Via Roma 1" };

        CustomerDto dto = (CustomerDto)customer;

        Assert.Equal("ACME", dto.Name);
        Assert.Null(dto.Email);
        Assert.Null(dto.VatNumber);
        Assert.Equal("Via Roma 1", dto.Address);
    }

    [Fact]
    public void DocumentLineRequest_From_EditModel_Normalizes_Description()
    {
        var line = new DocumentLineEditModel { Description = " Line ", Quantity = 2, UnitPrice = 3.5M };

        DocumentLineRequest request = (DocumentLineRequest)line;

        Assert.Equal("Line", request.Description);
        Assert.Equal(2M, request.Quantity);
        Assert.Equal(3.5M, request.UnitPrice);
    }
}
