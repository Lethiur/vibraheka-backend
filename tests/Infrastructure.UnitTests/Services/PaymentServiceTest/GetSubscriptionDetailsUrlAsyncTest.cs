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
        UserEntity user = new() { Id = "user-1", CustomerID = "cus-1" };

        _userRepositoryMock.Setup(x => x.GetByIdAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(user));
        _paymentRepositoryMock.Setup(x => x.GetSubscriptionPanelUrlAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success("https://portal.test"));

        // When
        Result<string> result = await _service.GetSubscriptionDetailsUrlAsync("user-1", CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo("https://portal.test"));
    }
}
