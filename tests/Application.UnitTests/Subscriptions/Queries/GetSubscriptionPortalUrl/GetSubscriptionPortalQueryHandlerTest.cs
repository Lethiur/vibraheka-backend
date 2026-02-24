using CSharpFunctionalExtensions;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Subscriptions.Queries.GetSubscriptionPortalUrl;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.Payments;

namespace VibraHeka.Application.UnitTests.Subscriptions.Queries.GetSubscriptionPortalUrl;

[TestFixture]
public class GetSubscriptionPortalQueryHandlerTest
{
    [Test]
    public async Task ShouldReturnPortalUrlForCurrentUser()
    {
        Mock<ICurrentUserService> currentUserMock = new();
        Mock<IPaymentService> paymentServiceMock = new();

        currentUserMock.Setup(x => x.UserId).Returns("user-1");
        paymentServiceMock.Setup(x => x.GetSubscriptionDetailsUrlAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success("https://portal.test"));

        GetSubscriptionPortalQueryHandler handler = new(currentUserMock.Object, paymentServiceMock.Object);

        Result<string> result = await handler.Handle(new GetSubscriptionPortalQuery(), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo("https://portal.test"));
    }
}
