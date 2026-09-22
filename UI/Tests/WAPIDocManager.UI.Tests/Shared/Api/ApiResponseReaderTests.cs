using System.Net;
using System.Text;
using WAPIDocManager.UI.Features.Documents.Entities;
using WAPIDocManager.UI.Shared.Api;

namespace WAPIDocManager.UI.Tests.Shared.Api;

/// <summary>
/// Conversione delle risposte in ApiException: ProblemDetails (GlobalExceptionHandler), ValidationProblemDetails,
/// body assente, non JSON o malformato.
/// </summary>
public class ApiResponseReaderTests
{
    private sealed record Sample(string Name);

    [Fact]
    public async Task ReadAsync_Success_Deserializes_CamelCase_Body()
    {
        using HttpResponseMessage response = CreateResponse(HttpStatusCode.OK, """{ "name": "ok" }""", "application/json");

        Sample result = await ApiResponseReader.ReadAsync<Sample>(response, CancellationToken.None);

        Assert.Equal("ok", result.Name);
    }

    [Fact]
    public async Task EnsureSuccessAsync_NotFound_Without_Body_Throws_Without_Detail()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.NotFound);

        ApiException exception = await Assert.ThrowsAsync<ApiException>(
            () => ApiResponseReader.EnsureSuccessAsync(response, CancellationToken.None));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Null(exception.Detail);
    }

    [Fact]
    public async Task EnsureSuccessAsync_ProblemDetails_Uses_Detail()
    {
        using HttpResponseMessage response = CreateResponse(
            HttpStatusCode.BadRequest,
            """{ "type": "BadRequest", "title": "Error", "status": 400, "detail": "Document already attached." }""",
            "application/problem+json");

        ApiException exception = await Assert.ThrowsAsync<ApiException>(
            () => ApiResponseReader.EnsureSuccessAsync(response, CancellationToken.None));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        Assert.Equal("Document already attached.", exception.Detail);
    }

    [Fact]
    public async Task EnsureSuccessAsync_ValidationProblemDetails_Joins_Errors()
    {
        using HttpResponseMessage response = CreateResponse(
            HttpStatusCode.BadRequest,
            """{ "title": "One or more validation errors occurred.", "status": 400, "errors": { "Email": [ "The Email field is required." ], "Password": [ "The Password field is required." ] } }""",
            "application/problem+json");

        ApiException exception = await Assert.ThrowsAsync<ApiException>(
            () => ApiResponseReader.EnsureSuccessAsync(response, CancellationToken.None));

        Assert.Contains("The Email field is required.", exception.Detail);
        Assert.Contains("The Password field is required.", exception.Detail);
    }

    [Fact]
    public async Task EnsureSuccessAsync_Non_Json_Body_Throws_Without_Detail()
    {
        using HttpResponseMessage response = CreateResponse(HttpStatusCode.InternalServerError, "oops", "text/plain");

        ApiException exception = await Assert.ThrowsAsync<ApiException>(
            () => ApiResponseReader.EnsureSuccessAsync(response, CancellationToken.None));

        Assert.Equal(HttpStatusCode.InternalServerError, exception.StatusCode);
        Assert.Null(exception.Detail);
    }

    [Fact]
    public async Task EnsureSuccessAsync_Malformed_Json_Throws_Without_Detail()
    {
        using HttpResponseMessage response = CreateResponse(HttpStatusCode.BadRequest, "{ not json", "application/json");

        ApiException exception = await Assert.ThrowsAsync<ApiException>(
            () => ApiResponseReader.EnsureSuccessAsync(response, CancellationToken.None));

        Assert.Null(exception.Detail);
    }

    private static HttpResponseMessage CreateResponse(HttpStatusCode statusCode, string content, string mediaType)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content, Encoding.UTF8, mediaType)
        };
    }
}
