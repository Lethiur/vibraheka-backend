using System.ComponentModel;

namespace VibraHeka.Infrastructure.Persistence.UnitTest.Catalog.Adapters.SellableItemAdapterTest;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class DeactivateSellableItemAsyncTest : GenericSellableItemAdapterTest
{
    [Test]
    [DisplayName("Should throw NotImplementedException when DeactivateSellableItemAsync is called")]
    public void ShouldThrowNotImplementedExceptionWhenCalled()
    {
        // Given: any referenceId — method has not been implemented yet
        string referenceId = "ref-id-deactivate-notimpl-001";

        // When / Then: calling DeactivateSellableItemAsync throws NotImplementedException immediately
        Assert.ThrowsAsync<NotImplementedException>(
            () => Adapter.DeactivateSellableItemAsync(referenceId, CancellationToken.None),
            $"Expected DeactivateSellableItemAsync to throw NotImplementedException for referenceId='{referenceId}'");

        RepositoryMock.VerifyNoOtherCalls();
        PriceRepositoryMock.VerifyNoOtherCalls();
    }
}
