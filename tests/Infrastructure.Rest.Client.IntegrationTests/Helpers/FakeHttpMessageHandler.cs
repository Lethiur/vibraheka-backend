using System.Net;

namespace VibraHeka.Infrastructure.Rest.Client.IntegrationTests.Helpers;

/// <summary>
/// Deterministic HTTP stub for integration tests.
/// Enqueue responses in order; each SendAsync call dequeues one.
/// </summary>
public sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Queue<Func<HttpRequestMessage, HttpResponseMessage>> _handlers = new();

    public List<HttpRequestMessage> SentRequests { get; } = new();

    public int PendingResponses => _handlers.Count;

    public void EnqueueResponse(HttpResponseMessage response)
    {
        _handlers.Enqueue(_ => response);
    }

    public void EnqueueStatusOnly(HttpStatusCode statusCode)
    {
        _handlers.Enqueue(_ => new HttpResponseMessage(statusCode));
    }

    public void EnqueueJson(HttpStatusCode statusCode, string json)
    {
        _handlers.Enqueue(_ => new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
        });
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        SentRequests.Add(request);
        if (_handlers.Count == 0)
        {
            throw new InvalidOperationException(
                "FakeHttpMessageHandler: no more responses queued. Enqueue a response before making an HTTP call.");
        }

        return Task.FromResult(_handlers.Dequeue()(request));
    }
}

