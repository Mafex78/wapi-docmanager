using WAPIDocManager.UI.Features.Documents.Entities;
using WAPIDocManager.UI.Features.Documents.Models;
using WAPIDocManager.UI.Features.Documents.Services;

namespace WAPIDocManager.UI.Tests.Features.Documents;

/// <summary>
/// Formato esatto della query string di ricerca: ordine dei parametri, escaping, liste ripetute senza duplicati, parametri omessi.
/// </summary>
public class DocumentQueryStringBuilderTests
{
    [Fact]
    public void Build_Default_Filter_Contains_Paging_And_Default_Sort()
    {
        string query = DocumentQueryStringBuilder.Build(new DocumentFilter());

        Assert.Equal("Page=1&PageSize=20&SortBy=Date&SortDirection=desc", query);
    }

    [Fact]
    public void Build_Escapes_Text_And_Repeats_Lists_Without_Duplicates()
    {
        var filter = new DocumentFilter
        {
            SortBy = DocumentSortFields.CustomerName,
            SortDirection = SortDirection.Asc,
            PlainText = " 2026-09-14 ",
            CustomerName = "Rossi & Figli",
            Types = { DocumentType.Proforma, DocumentType.Proforma },
            Statuses = { DocumentStatus.Draft, DocumentStatus.Rejected }
        };

        string query = DocumentQueryStringBuilder.Build(filter);

        Assert.Equal(
            "Page=1&PageSize=20&SortBy=Customer.Name&SortDirection=asc&PlainText=2026-09-14&CustomerName=Rossi%20%26%20Figli&DocumentTypes=1&DocumentStatuses=0&DocumentStatuses=4",
            query);
    }

    [Fact]
    public void Build_Without_SortBy_Omits_Sorting()
    {
        string query = DocumentQueryStringBuilder.Build(new DocumentFilter { SortBy = null });

        Assert.Equal("Page=1&PageSize=20", query);
    }
}
