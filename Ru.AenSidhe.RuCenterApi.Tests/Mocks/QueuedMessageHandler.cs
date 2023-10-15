

using System.Net;

namespace Ru.AenSidhe.RuCenterApi.Tests.Mocks;

public class QueuedMessageHandler : DelegatingHandler
{
    private readonly Queue<HttpResponseMessage> _messages;

    public QueuedMessageHandler(params HttpResponseMessage[] messages)
    {
        _messages = new(messages);
    }

    public Queue<(HttpRequestMessage, string?)> Requests { get; } = new();

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await EnqueueRequest(request, cancellationToken);
        return _messages.Dequeue();
    }

    protected async Task EnqueueRequest(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Enqueue((request, request.Content == null ? null : await request.Content.ReadAsStringAsync(cancellationToken)));
    }

    internal void Add(HttpResponseMessage message) => _messages.Enqueue(message);
}

public sealed class ExceptionHandler : QueuedMessageHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await EnqueueRequest(request, cancellationToken);
        await Task.Yield();
        throw new Exception("BOOM");
    }
}

public sealed class UnauthorizedHandler : QueuedMessageHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await EnqueueRequest(request, cancellationToken);
        return new HttpResponseMessage(HttpStatusCode.Unauthorized);
    }
}