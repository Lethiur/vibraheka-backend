using CSharpFunctionalExtensions;
using Moq;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Infrastructure.UnitTests.Services.PaymentServiceTest;

[TestFixture]
public class GetSubscriptionDetailsUrlAsyncTest : GenericPaymentServiceTest
{
    [Test]
    public async Task ShouldReturnPortalUrlFromPaymentRepository()
    {
        // Given
        UserProfileEntity userProfile = new() { Id = "user-1", CustomerID = "cus-1" };

        _userRepositoryMock.Setup(x => x.GetByIdAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(userProfile));
        _paymentRepositoryMock.Setup(x => x.GetSubscriptionPanelUrlAsync(userProfile, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success("https://portal.test"));

        // When
        Result<string> result = await _service.GetSubscriptionDetailsUrlAsync("user-1", CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo("https://portal.test"));
    }
}
