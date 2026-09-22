using System.Net.Http.Headers;
using System.Net;
using System.Text;

namespace WAPIDocManager.UI.Tests.TestSupport;

/// <summary>
/// Snapshot di una richiesta intercettata: il body viene letto subito perché il contenuto della richiesta
/// viene rilasciato dal chiamante (using) al termine della chiamata.
/// </summary>
public sealed record CapturedRequest(
    HttpMethod Method,
    Uri Uri,
    string? Body,
    AuthenticationHeaderValue? Authorization);

/// <summary>
/// HttpMessageHandler che registra le richieste e restituisce una risposta predefinita
/// </summary>
public sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

    public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        _responder = responder;
    }

    public List<CapturedRequest> Requests { get; } = new();

    public CapturedRequest LastRequest => Requests[^1];

    public static FakeHttpMessageHandler Returning(
        HttpStatusCode statusCode,
        string? content = null,
        string mediaType = "application/json")
    {
        return new FakeHttpMessageHandler(_ =>
        {
            var response = new HttpResponseMessage(statusCode);

            if (content is not null)
            {
                response.Content = new StringContent(content, Encoding.UTF8, mediaType);
            }

            return response;
        });
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        string? body = request.Content is null
            ? null
            : await request.Content.ReadAsStringAsync(cancellationToken);

        Requests.Add(new CapturedRequest(request.Method, request.RequestUri!, body, request.Headers.Authorization));

        return _responder(request);
    }
}
