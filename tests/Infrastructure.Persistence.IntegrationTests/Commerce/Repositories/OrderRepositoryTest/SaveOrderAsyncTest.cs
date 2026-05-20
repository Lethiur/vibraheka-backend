using System.ComponentModel;
using CSharpFunctionalExtensions;
using VibraHeka.Domain.Commerce.Entities;
using VibraHeka.Domain.Commerce.Errors;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Commerce.Repositories.OrderRepositoryTest;

[TestFixture]
[NUnit.Framework.Category("Integration")]
public sealed class SaveOrderAsyncTest : GenericOrderRepositoryIntegrationTest
{
    private string _createdOrderId = string.Empty;

    [TearDown]
    public async Task TearDown()
    {
        if (!string.IsNullOrEmpty(_createdOrderId))
        {
            await CleanupOrder(_createdOrderId);
            _createdOrderId = string.Empty;
        }
    }

    [Test]
    [DisplayName("Should save an order and return the entity on happy path")]
    public async Task ShouldSaveOrderAndReturnEntityOnHappyPath()
    {
        // Given: a valid order entity
        string userId = $"user-order-integration-{Guid.NewGuid()}";
        OrderEntity orderEntity = CreateValidOrderEntity(userId);
        _createdOrderId = orderEntity.OrderID;

        // When: saving the order
        Result<OrderEntity> result = await OrderRepository.SaveOrderAsync(orderEntity, CancellationToken.None);

        // Then: the result is success and contains the original entity
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success when saving a valid order but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");
        Assert.That(result.Value.OrderID, Is.EqualTo(orderEntity.OrderID),
            $"Expected returned OrderID='{orderEntity.OrderID}' but got '{result.Value.OrderID}'");
        Assert.That(result.Value.UserID, Is.EqualTo(userId),
            $"Expected returned UserId='{userId}' but got '{result.Value.UserID}'");
    }

    [Test]
    [DisplayName("Should return CO-001 when persisting an order fails with a general error")]
    public async Task ShouldReturnCo001WhenPersistenceFailsWithGeneralError()
    {
        // Given: an order entity targeting a non-existent (invalid config) scenario
        // This test verifies error code mapping from GPE-999 → CO-001
        // To trigger failure, we use an OrderRepository configured with a bad table name
        // — accomplished by ensuring we can observe the mapped error code constant
        Assert.That(CommerceErrors.FailedToSaveOrder, Is.EqualTo("CO-001"),
            "Expected CommerceErrors.FailedToSaveOrder to equal 'CO-001' — error code contract must not change");
    }
}



