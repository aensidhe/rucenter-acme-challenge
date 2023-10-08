

namespace Ru.AenSidhe.RuCenterApi.Tests.Mocks;
public sealed class HttpClientFactory : IHttpClientFactory
{
    private readonly HttpMessageHandler _handler;

    public HttpClientFactory(HttpMessageHandler handler)
    {
        _handler = handler;
    }

    public HttpClientFactory(params HttpResponseMessage[] messages)
        : this(new QueuedMessageHandler(messages))
    {
    }

    public HttpClient CreateClient(string name)
    {
        return new HttpClient(_handler);
    }
}
