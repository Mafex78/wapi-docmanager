using WAPIDocManager.UI.Features.Documents.Entities;

namespace WAPIDocManager.UI.Tests.Features.Documents;

/// <summary>
/// Regole di stato del documento (DocumentRules), speculari al dominio di WAPIDocument:
/// vanno aggiornate insieme a <c>WAPIDocument.Domain/Entities/Documents/Document.cs</c>.
/// </summary>
public class DocumentRulesTests
{
    [Theory]
    [InlineData(DocumentStatus.Draft, true)]
    [InlineData(DocumentStatus.Ready, true)]
    [InlineData(DocumentStatus.Sent, false)]
    [InlineData(DocumentStatus.Approved, false)]
    [InlineData(DocumentStatus.Rejected, false)]
    public void CanEdit_And_CanDelete_Allowed_Only_In_Draft_And_Ready(DocumentStatus status, bool expected)
    {
        Assert.Equal(expected, DocumentRules.CanEdit(status));
        Assert.Equal(expected, DocumentRules.CanDelete(status));
    }

    [Theory]
    [InlineData(DocumentStatus.Draft, new[] { DocumentStatus.Ready })]
    [InlineData(DocumentStatus.Ready, new[] { DocumentStatus.Sent })]
    [InlineData(DocumentStatus.Sent, new[] { DocumentStatus.Approved, DocumentStatus.Rejected })]
    [InlineData(DocumentStatus.Approved, new DocumentStatus[] { })]
    [InlineData(DocumentStatus.Rejected, new DocumentStatus[] { })]
    public void GetNextStatuses_Returns_Allowed_Transitions(DocumentStatus status, DocumentStatus[] expected)
    {
        Assert.Equal(expected, DocumentRules.GetNextStatuses(status));
    }

    [Fact]
    public void IsComplete_With_Customer_And_Valid_Lines_Returns_True()
    {
        Assert.True(DocumentRules.IsComplete(CreateCompleteDocument()));
    }

    [Fact]
    public void IsComplete_Without_VatNumber_Returns_False()
    {
        Document document = CreateCompleteDocument() with
        {
            Customer = new Customer { Name = "ACME" }
        };

        Assert.False(DocumentRules.IsComplete(document));
    }

    [Fact]
    public void IsComplete_Without_Lines_Returns_False()
    {
        Document document = CreateCompleteDocument() with
        {
            Lines = new List<DocumentLine>()
        };

        Assert.False(DocumentRules.IsComplete(document));
    }

    [Fact]
    public void IsComplete_With_Invalid_Line_Returns_False()
    {
        Document document = CreateCompleteDocument() with
        {
            Lines = new List<DocumentLine>
            {
                new() { Description = "Line", Quantity = 0, UnitPrice = 10 }
            }
        };

        Assert.False(DocumentRules.IsComplete(document));
    }

    private static Document CreateCompleteDocument()
    {
        return new Document
        {
            Id = "1",
            Customer = new Customer { Name = "ACME", VatNumber = "IT01234567890" },
            Lines = new List<DocumentLine>
            {
                new() { Description = "Line", Quantity = 1, UnitPrice = 10, Total = 10 }
            }
        };
    }
}
