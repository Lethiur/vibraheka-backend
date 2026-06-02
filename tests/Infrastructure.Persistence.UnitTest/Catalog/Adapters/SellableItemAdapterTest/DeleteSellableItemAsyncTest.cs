using System.ComponentModel;

namespace VibraHeka.Infrastructure.Persistence.UnitTest.Catalog.Adapters.SellableItemAdapterTest;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class DeleteSellableItemAsyncTest : GenericSellableItemAdapterTest
{
    [Test]
    [DisplayName("Should throw NotImplementedException when DeleteSellableItemAsync is called")]
    public void ShouldThrowNotImplementedExceptionWhenCalled()
    {
        // Given: any referenceId — method has not been implemented yet
        string referenceId = "ref-id-delete-notimpl-001";

        // When / Then: calling DeleteSellableItemAsync throws NotImplementedException immediately
        Assert.ThrowsAsync<NotImplementedException>(
            () => Adapter.DeleteSellableItemAsync(referenceId, CancellationToken.None),
            $"Expected DeleteSellableItemAsync to throw NotImplementedException for referenceId='{referenceId}'");

        RepositoryMock.VerifyNoOtherCalls();
        PriceRepositoryMock.VerifyNoOtherCalls();
    }
}

