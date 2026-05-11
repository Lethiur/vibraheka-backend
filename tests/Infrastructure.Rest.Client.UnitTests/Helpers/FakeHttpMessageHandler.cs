namespace Infrastructure.Rest.Client.UnitTests.Helpers;

/// <summary>
/// Fake HttpMessageHandler that returns pre-configured responses in FIFO order.
/// Use EnqueueResponse to add responses before invoking the class under test.
/// </summary>
public sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Queue<HttpResponseMessage> Responses = new();
    private readonly List<HttpRequestMessage> SentRequests = new();

    public IReadOnlyList<HttpRequestMessage> CapturedRequests => SentRequests.AsReadOnly();
    public int RequestCount => SentRequests.Count;

    public void EnqueueResponse(HttpResponseMessage response)
    {
        Responses.Enqueue(response);
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        SentRequests.Add(request);
        if (Responses.Count == 0)
        {
            throw new InvalidOperationException(
                $"No response configured for request: {request.Method} {request.RequestUri}");
        }

        return Task.FromResult(Responses.Dequeue());
    }
}

