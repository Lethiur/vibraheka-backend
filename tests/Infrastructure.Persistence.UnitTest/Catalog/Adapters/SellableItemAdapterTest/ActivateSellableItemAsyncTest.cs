using System.ComponentModel;

namespace VibraHeka.Infrastructure.Persistence.UnitTest.Catalog.Adapters.SellableItemAdapterTest;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class ActivateSellableItemAsyncTest : GenericSellableItemAdapterTest
{
    [Test]
    [DisplayName("Should throw NotImplementedException when ActivateSellableItemAsync is called")]
    public void ShouldThrowNotImplementedExceptionWhenCalled()
    {
        // Given: any referenceId — method has not been implemented yet
        string referenceId = "ref-id-activate-notimpl-001";

        // When / Then: calling ActivateSellableItemAsync throws NotImplementedException immediately
        Assert.ThrowsAsync<NotImplementedException>(
            () => Adapter.ActivateSellableItemAsync(referenceId, CancellationToken.None),
            $"Expected ActivateSellableItemAsync to throw NotImplementedException for referenceId='{referenceId}'");

        RepositoryMock.VerifyNoOtherCalls();
        PriceRepositoryMock.VerifyNoOtherCalls();
    }
}
