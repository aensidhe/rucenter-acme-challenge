

namespace Ru.AenSidhe.RuCenterApi.Tests.Mocks;

public sealed class QueuedMessageHandler : DelegatingHandler
{
    private readonly Queue<HttpResponseMessage> _messages;

    public QueuedMessageHandler(params HttpResponseMessage[] messages)
    {
        _messages = new Queue<HttpResponseMessage>(messages);
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_messages.Dequeue());
    }
}