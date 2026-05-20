using System.ComponentModel;
using CSharpFunctionalExtensions;
using VibraHeka.Domain.Commerce.Entities;
using VibraHeka.Domain.Commerce.Errors;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Commerce.Repositories.OrderLineRepositoryTest;

[TestFixture]
[NUnit.Framework.Category("Integration")]
public sealed class SaveOrderLinesAsyncTest : GenericOrderLineRepositoryIntegrationTest
{
    private readonly List<string> _createdOrderLineIds = [];

    [TearDown]
    public async Task TearDown()
    {
        foreach (string id in _createdOrderLineIds)
        {
            await CleanupOrderLine(id);
        }

        _createdOrderLineIds.Clear();
    }

    [Test]
    [DisplayName("Should save a single order line and return it in the result collection")]
    public async Task ShouldSaveSingleOrderLineAndReturnItInCollection()
    {
        // Given: a single valid order line entity
        string userId = $"user-orderline-integration-{Guid.NewGuid()}";
        string orderId = $"order-integration-{Guid.NewGuid()}";
        OrderLineEntity lineEntity = CreateValidOrderLineEntity(orderId, userId);
        _createdOrderLineIds.Add(lineEntity.OrderLineID);

        List<OrderLineEntity> lines = [lineEntity];

        // When: saving the order lines collection
        Result<IReadOnlyCollection<OrderLineEntity>> result =
            await OrderLineRepository.SaveOrderLinesAsync(lines, CancellationToken.None);

        // Then: the result is success and returns the original collection
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success when saving a valid order line but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");
        Assert.That(result.Value, Has.Count.EqualTo(1),
            $"Expected 1 order line returned but got {result.Value.Count}");
        Assert.That(result.Value.First().OrderLineID, Is.EqualTo(lineEntity.OrderLineID),
            $"Expected OrderLineID='{lineEntity.OrderLineID}' but got '{result.Value.First().OrderLineID}'");
    }

    [Test]
    [DisplayName("Should save multiple order lines and return them all in the result collection")]
    public async Task ShouldSaveMultipleOrderLinesAndReturnThemAll()
    {
        // Given: two valid order line entities
        string userId = $"user-orderline-multi-integration-{Guid.NewGuid()}";
        string orderId = $"order-multi-integration-{Guid.NewGuid()}";
        OrderLineEntity firstLine = CreateValidOrderLineEntity(orderId, userId);
        OrderLineEntity secondLine = CreateValidOrderLineEntity(orderId, userId);
        _createdOrderLineIds.Add(firstLine.OrderLineID);
        _createdOrderLineIds.Add(secondLine.OrderLineID);

        List<OrderLineEntity> lines = [firstLine, secondLine];

        // When: saving both order lines
        Result<IReadOnlyCollection<OrderLineEntity>> result =
            await OrderLineRepository.SaveOrderLinesAsync(lines, CancellationToken.None);

        // Then: the result is success with both lines returned
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success when saving two order lines but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");
        Assert.That(result.Value, Has.Count.EqualTo(2),
            $"Expected 2 order lines returned but got {result.Value.Count}");
    }

    [Test]
    [DisplayName("Should map failure to CO-002 error code as defined in CommerceErrors")]
    public void ShouldUseCorrectErrorCodeForOrderLineSaveFailure()
    {
        // Given / When / Then: the error code contract is verified as a constant
        Assert.That(CommerceErrors.FailedToSaveOrderLines, Is.EqualTo("CO-002"),
            "Expected CommerceErrors.FailedToSaveOrderLines to equal 'CO-002' — error code contract must not change");
    }
}



