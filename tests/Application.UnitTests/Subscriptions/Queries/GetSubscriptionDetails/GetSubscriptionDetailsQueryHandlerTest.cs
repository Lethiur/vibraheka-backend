using CSharpFunctionalExtensions;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Subscriptions.Queries.GetSubscriptionDetails;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.Orders;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Application.UnitTests.Subscriptions.Queries.GetSubscriptionDetails;

[TestFixture]
public class GetSubscriptionDetailsQueryHandlerTest
{
    [Test]
    public async Task ShouldReturnSubscriptionForCurrentUser()
    {
        Mock<ISubscriptionService> subscriptionServiceMock = new();
        Mock<ICurrentUserService> currentUserMock = new();
        SubscriptionEntity expected = new() { UserID = "user-1", SubscriptionID = "sub-1" };

        currentUserMock.Setup(x => x.UserId).Returns("user-1");
        subscriptionServiceMock.Setup(x => x.GetSubscriptionForUser("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(expected));

        GetSubscriptionDetailsQueryHandler handler = new(subscriptionServiceMock.Object, currentUserMock.Object);

        Result<SubscriptionEntity> result = await handler.Handle(new GetSubscriptionDetailsQuery(), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.SubscriptionID, Is.EqualTo("sub-1"));
    }
}
