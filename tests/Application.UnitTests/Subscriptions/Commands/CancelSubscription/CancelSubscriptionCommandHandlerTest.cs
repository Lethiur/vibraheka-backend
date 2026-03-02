using CSharpFunctionalExtensions;
using MediatR;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Subscriptions.Commands.CancelSubscription;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.Orders;

namespace VibraHeka.Application.UnitTests.Subscriptions.Commands.CancelSubscription;

[TestFixture]
public class CancelSubscriptionCommandHandlerTest
{
    [Test]
    public async Task ShouldCallSubscriptionServiceWithCurrentUser()
    {
        Mock<ICurrentUserService> currentUserMock = new();
        Mock<ISubscriptionService> subscriptionServiceMock = new();

        currentUserMock.Setup(x => x.UserId).Returns("user-1");
        subscriptionServiceMock.Setup(x => x.CancelSubscriptionForUser("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Unit.Value));

        CancelSubscriptionCommandHandler handler = new(currentUserMock.Object, subscriptionServiceMock.Object);

        Result<Unit> result = await handler.Handle(new CancelSubscriptionCommand(), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        subscriptionServiceMock.Verify(x => x.CancelSubscriptionForUser("user-1", It.IsAny<CancellationToken>()), Times.Once);
    }
}
