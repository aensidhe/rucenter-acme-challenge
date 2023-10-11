

using System.Collections;
using System.Net;

namespace Ru.AenSidhe.RuCenterApi.Tests.Mocks;
public sealed class HttpClientFactory : IHttpClientFactory, IEnumerable
{
    private readonly HttpMessageHandler _handler;

    public HttpClientFactory(HttpMessageHandler handler)
    {
        _handler = handler;
    }

    public HttpClientFactory(params HttpResponseMessage[] messages)
    {
        #pragma warning disable IDISP003
        _handler = new QueuedMessageHandler(messages);
        #pragma warning restore IDISP003
    }

    public void Add(HttpResponseMessage message)
    {
        if (_handler is QueuedMessageHandler q)
            q.Add(message);
    }

    public (HttpRequestMessage, string?) PopSeenRequest()
    {
        if (_handler is QueuedMessageHandler q)
            return q.Requests.Dequeue();

        throw new InvalidOperationException();
    }

    public HttpClient CreateClient(string _) => new(_handler);

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }

    public static IHttpClientFactory BOOMFactory { get; } = new HttpClientFactory(new ExceptionHandler());

    public static IHttpClientFactory Unauthorized { get; } = new HttpClientFactory(new UnauthorizedHandler());

    private sealed class ExceptionHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await Task.Yield();
            throw new Exception("BOOM");
        }
    }

    private sealed class UnauthorizedHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized));
        }
    }
}
