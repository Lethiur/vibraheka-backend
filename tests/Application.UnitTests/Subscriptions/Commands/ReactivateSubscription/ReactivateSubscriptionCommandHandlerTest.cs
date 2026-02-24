using CSharpFunctionalExtensions;
using MediatR;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Subscriptions.Commands.ReactivateSubscription;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.Orders;

namespace VibraHeka.Application.UnitTests.Subscriptions.Commands.ReactivateSubscription;

[TestFixture]
public class ReactivateSubscriptionCommandHandlerTest
{
    [Test]
    public async Task ShouldMapServiceSuccessToUnitValue()
    {
        Mock<ICurrentUserService> currentUserMock = new();
        Mock<ISubscriptionService> subscriptionServiceMock = new();

        currentUserMock.Setup(x => x.UserId).Returns("user-1");
        subscriptionServiceMock.Setup(x => x.ReactivateSubscription("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Unit.Value));

        ReactivateSubscriptionCommandHandler handler = new(currentUserMock.Object, subscriptionServiceMock.Object);

        Result<Unit> result = await handler.Handle(new ReactivateSubscriptionCommand(), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(Unit.Value));
    }

    [Test]
    public async Task ShouldPropagateServiceFailure()
    {
        Mock<ICurrentUserService> currentUserMock = new();
        Mock<ISubscriptionService> subscriptionServiceMock = new();

        currentUserMock.Setup(x => x.UserId).Returns("user-1");
        subscriptionServiceMock.Setup(x => x.ReactivateSubscription("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Unit>("S-002"));

        ReactivateSubscriptionCommandHandler handler = new(currentUserMock.Object, subscriptionServiceMock.Object);

        Result<Unit> result = await handler.Handle(new ReactivateSubscriptionCommand(), CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("S-002"));
    }
}
