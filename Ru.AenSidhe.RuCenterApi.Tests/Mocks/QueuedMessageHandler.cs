

namespace Ru.AenSidhe.RuCenterApi.Tests.Mocks;

public sealed class QueuedMessageHandler : DelegatingHandler
{
    private readonly Queue<HttpResponseMessage> _messages;

    public QueuedMessageHandler(params HttpResponseMessage[] messages)
    {
        _messages = new(messages);
    }

    public Queue<(HttpRequestMessage, string?)> Requests { get; } = new();

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Enqueue((request, request.Content == null ? null : await request.Content.ReadAsStringAsync(cancellationToken)));
        return _messages.Dequeue();
    }

    internal void Add(HttpResponseMessage message) => _messages.Enqueue(message);
}