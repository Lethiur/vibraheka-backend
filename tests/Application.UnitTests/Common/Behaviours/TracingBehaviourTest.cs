using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Core.Internal.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Common.Behaviours;
using VibraHeka.Domain.Common.Interfaces;

namespace VibraHeka.Application.UnitTests.Common.Behaviours;

public record TestTracingRequest() : IRequest<string>;

[TestFixture]
public class TracingBehaviourTest
{
    [SetUp]
    public void SetUp()
    {
        AWSXRayRecorder.Instance.TraceContext.SetEntity(new Segment("unit-test"));
    }

    [Test]
    public async Task ShouldCallNextWhenNoExceptionIsThrown()
    {
        // Given
        Mock<ITracer> tracerMock = new();
        Mock<ILogger<TracingBehaviour<TestTracingRequest, string>>> loggerMock = new();
        tracerMock.Setup(x => x.GetTraceId()).Returns("trace-id");

        TracingBehaviour<TestTracingRequest, string> behaviour = new(tracerMock.Object, loggerMock.Object);

        // When
        string result = await behaviour.Handle(new TestTracingRequest(), _ => Task.FromResult("ok"), CancellationToken.None);

        // Then
        Assert.That(result, Is.EqualTo("ok"));
    }

    [Test]
    public void ShouldAddExceptionToTracerWhenNextThrows()
    {
        // Given
        Mock<ITracer> tracerMock = new();
        Mock<ILogger<TracingBehaviour<TestTracingRequest, string>>> loggerMock = new();
        tracerMock.Setup(x => x.GetTraceId()).Returns("trace-id");

        TracingBehaviour<TestTracingRequest, string> behaviour = new(tracerMock.Object, loggerMock.Object);

        // When
        TestDelegate action = () => behaviour.Handle(
                new TestTracingRequest(),
                _ => throw new InvalidOperationException("boom"),
                CancellationToken.None)
            .GetAwaiter().GetResult();

        // Then
        Assert.Throws<InvalidOperationException>(action);
        tracerMock.Verify(x => x.AddException(It.IsAny<InvalidOperationException>()), Times.Once);
    }
}
