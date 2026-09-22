using System.Net;
using System.Text.Json;
using WAPIDocManager.UI.Features.Documents.Entities;
using WAPIDocManager.UI.Features.Documents.Models;
using WAPIDocManager.UI.Features.Documents.Services;
using WAPIDocManager.UI.Shared.Api;
using WAPIDocManager.UI.Tests.TestSupport;

namespace WAPIDocManager.UI.Tests.Features.Documents;

/// <summary>
/// Contratto HTTP verso WAPIDocument: URL, metodi, query string, body JSON (enum numerici, EUR, date UTC) e mapping delle risposte.
/// Il JSON di esempio riproduce la forma delle risposte reali del backend (camelCase): aggiornarlo se cambiano i DTO.
/// </summary>
public class DocumentApiServiceTests
{
    private const string DocumentJson = """
        {
          "id": "abc",
          "number": "3f2a9c1e-0000-0000-0000-000000000000",
          "date": "2026-09-14T00:00:00Z",
          "customer": { "name": "ACME", "email": null, "vatNumber": "IT123", "address": "Via Roma 1" },
          "currency": "EUR",
          "type": 1,
          "status": 2,
          "documentLines": [ { "description": "Line", "quantity": 2, "unitPrice": 10.5, "total": 21 } ],
          "total": 21,
          "linkedDocuments": [ { "targetDocumentId": "def" } ]
        }
        """;

    [Fact]
    public async Task GetByIdAsync_Calls_Document_Endpoint_And_Maps_Response()
    {
        (DocumentApiService service, FakeHttpMessageHandler handler) = CreateService(HttpStatusCode.OK, DocumentJson);

        Document document = await service.GetByIdAsync("abc");

        Assert.Equal(HttpMethod.Get, handler.LastRequest.Method);
        Assert.Equal("/api/v1/documents/abc", handler.LastRequest.Uri.AbsolutePath);
        Assert.Equal("abc", document.Id);
        Assert.Equal(new DateTime(2026, 9, 14, 0, 0, 0, DateTimeKind.Utc), document.Date);
        Assert.Equal(DocumentType.Proforma, document.Type);
        Assert.Equal(DocumentStatus.Sent, document.Status);
        Assert.Equal("ACME", document.Customer.Name);
        Assert.Equal("IT123", document.Customer.VatNumber);
        DocumentLine line = Assert.Single(document.Lines);
        Assert.Equal(10.5M, line.UnitPrice);
        Assert.Equal(21M, document.Total);
        Assert.Equal("def", Assert.Single(document.LinkedDocuments).TargetDocumentId);
    }

    [Fact]
    public async Task FindPagedAsync_Sends_Filter_As_Query_String_And_Maps_Page()
    {
        string pageJson = $$"""{ "pageSize": 10, "currentPage": 2, "totalItems": 11, "totalPages": 2, "items": [ {{DocumentJson}} ] }""";
        (DocumentApiService service, FakeHttpMessageHandler handler) = CreateService(HttpStatusCode.OK, pageJson);
        var filter = new DocumentFilter
        {
            Page = 2,
            PageSize = 10,
            SortBy = DocumentSortFields.Total,
            SortDirection = SortDirection.Desc,
            CustomerName = "Acme & Co",
            Types = { DocumentType.Quote, DocumentType.SalesOrder },
            Statuses = { DocumentStatus.Sent }
        };

        PagedResult<Document> result = await service.FindPagedAsync(filter);

        Assert.Equal(
            "/api/v1/documents?Page=2&PageSize=10&SortBy=Total&SortDirection=desc&CustomerName=Acme%20%26%20Co&DocumentTypes=0&DocumentTypes=2&DocumentStatuses=2",
            handler.LastRequest.Uri.PathAndQuery);
        Assert.Equal(2, result.CurrentPage);
        Assert.Equal(11, result.TotalItems);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal("abc", Assert.Single(result.Items).Id);
    }

    [Fact]
    public async Task CreateAsync_Posts_Euro_Currency_Customer_And_Lines()
    {
        (DocumentApiService service, FakeHttpMessageHandler handler) = CreateService(HttpStatusCode.OK, DocumentJson);
        var model = new DocumentEditModel
        {
            Type = DocumentType.Proforma,
            Customer = new CustomerEditModel { Name = "  ACME  ", Email = " ", VatNumber = "IT123" },
            Lines = { new DocumentLineEditModel { Description = "Line", Quantity = 2, UnitPrice = 10.5M } }
        };

        await service.CreateAsync(model);

        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal("/api/v1/documents", handler.LastRequest.Uri.AbsolutePath);

        using JsonDocument body = JsonDocument.Parse(handler.LastRequest.Body!);
        JsonElement root = body.RootElement;
        Assert.Equal(1, root.GetProperty("type").GetInt32());
        Assert.Equal("EUR", root.GetProperty("currency").GetString());
        Assert.Equal("ACME", root.GetProperty("customer").GetProperty("name").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("customer").GetProperty("email").ValueKind);
        JsonElement line = root.GetProperty("documentLines")[0];
        Assert.Equal("Line", line.GetProperty("description").GetString());
        Assert.Equal(2M, line.GetProperty("quantity").GetDecimal());
        Assert.Equal(10.5M, line.GetProperty("unitPrice").GetDecimal());
    }

    [Fact]
    public async Task UpdateAsync_Puts_Date_As_Utc_Midnight()
    {
        (DocumentApiService service, FakeHttpMessageHandler handler) = CreateService(HttpStatusCode.OK, DocumentJson);
        var model = new DocumentEditModel { Date = new DateTime(2026, 9, 14, 15, 30, 0, DateTimeKind.Unspecified) };

        await service.UpdateAsync("abc", model);

        Assert.Equal(HttpMethod.Put, handler.LastRequest.Method);
        Assert.Equal("/api/v1/documents/abc", handler.LastRequest.Uri.AbsolutePath);

        using JsonDocument body = JsonDocument.Parse(handler.LastRequest.Body!);
        Assert.Equal("2026-09-14T00:00:00Z", body.RootElement.GetProperty("date").GetString());
        Assert.Equal("EUR", body.RootElement.GetProperty("currency").GetString());
    }

    [Fact]
    public async Task UpdateStatusAsync_Puts_Status_As_Number()
    {
        (DocumentApiService service, FakeHttpMessageHandler handler) = CreateService(HttpStatusCode.OK, DocumentJson);

        await service.UpdateStatusAsync("abc", DocumentStatus.Approved);

        Assert.Equal(HttpMethod.Put, handler.LastRequest.Method);
        Assert.Equal("/api/v1/documents/abc/status", handler.LastRequest.Uri.AbsolutePath);
        Assert.Equal("3", handler.LastRequest.Body);
    }

    [Fact]
    public async Task DeleteAsync_Sends_Delete()
    {
        (DocumentApiService service, FakeHttpMessageHandler handler) = CreateService(HttpStatusCode.NoContent);

        await service.DeleteAsync("abc");

        Assert.Equal(HttpMethod.Delete, handler.LastRequest.Method);
        Assert.Equal("/api/v1/documents/abc", handler.LastRequest.Uri.AbsolutePath);
    }

    [Fact]
    public async Task GenerateFromAsync_Posts_Target_Type()
    {
        (DocumentApiService service, FakeHttpMessageHandler handler) = CreateService(HttpStatusCode.OK, DocumentJson);

        await service.GenerateFromAsync("abc", DocumentType.SalesOrder);

        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal("/api/v1/documents/abc/generation", handler.LastRequest.Uri.AbsolutePath);
        using JsonDocument body = JsonDocument.Parse(handler.LastRequest.Body!);
        Assert.Equal(2, body.RootElement.GetProperty("documentType").GetInt32());
    }

    [Fact]
    public async Task AttachAsync_Posts_Id_And_Returns_Links()
    {
        (DocumentApiService service, FakeHttpMessageHandler handler) = CreateService(
            HttpStatusCode.OK,
            """{ "linkedDocuments": [ { "targetDocumentId": "def" } ] }""");

        IReadOnlyList<DocumentLink> links = await service.AttachAsync("abc", "def");

        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal("/api/v1/documents/abc/attachments", handler.LastRequest.Uri.AbsolutePath);
        using JsonDocument body = JsonDocument.Parse(handler.LastRequest.Body!);
        Assert.Equal("def", body.RootElement.GetProperty("id").GetString());
        Assert.Equal("def", Assert.Single(links).TargetDocumentId);
    }

    [Fact]
    public async Task UpdateAsync_Bad_Request_Throws_ApiException_With_Detail()
    {
        (DocumentApiService service, _) = CreateService(
            HttpStatusCode.BadRequest,
            """{ "type": "BadRequest", "title": "Error", "status": 400, "detail": "Cannot edit a document in status Sent." }""",
            "application/problem+json");

        ApiException exception = await Assert.ThrowsAsync<ApiException>(() => service.UpdateAsync("abc", new DocumentEditModel()));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        Assert.Equal("Cannot edit a document in status Sent.", exception.Detail);
    }

    private static (DocumentApiService Service, FakeHttpMessageHandler Handler) CreateService(
        HttpStatusCode statusCode,
        string? content = null,
        string mediaType = "application/json")
    {
        FakeHttpMessageHandler handler = FakeHttpMessageHandler.Returning(statusCode, content, mediaType);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://localhost:7273/") };

        return (new DocumentApiService(httpClient), handler);
    }
}
