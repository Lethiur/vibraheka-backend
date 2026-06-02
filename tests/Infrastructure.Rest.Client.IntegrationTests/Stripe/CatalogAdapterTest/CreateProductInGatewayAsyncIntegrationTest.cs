using CSharpFunctionalExtensions;
using NMoneys;
using VibraHeka.Application.Catalog.Models;
using VibraHeka.Domain.Catalog.Entities;

namespace VibraHeka.Infrastructure.Rest.Client.IntegrationTests.Stripe.CatalogAdapterTest;

[TestFixture]
[NUnit.Framework.Category("Integration")]
public sealed class CreateProductInGatewayAsyncIntegrationTest : GenericCatalogAdapterIntegrationTest
{
    private string? CreatedProductId;
    private string? CreatedPriceId;

    [TearDown]
    public async Task TearDown()
    {
        // Cleanup in order: archive price first, then delete product (Stripe constraint)
        if (CreatedPriceId is not null)
        {
            await CleanupStripePrice(CreatedPriceId);
            CreatedPriceId = null;
        }

        if (CreatedProductId is not null)
        {
            await CleanupStripeProduct(CreatedProductId);
            CreatedProductId = null;
        }
    }

    [Test]
    [Description("Should return success with non-empty ProductGatewayID and ProductGatewayPriceID when Stripe test-mode creates a product and price")]
    public async Task ShouldReturnSuccessWithNonEmptyGatewayIdsWhenStripeCreatesProductAndPrice()
    {
        // Given: valid product and price entities
        ProductEntity productEntity = new ProductEntity
        {
            ID = Guid.NewGuid().ToString(),
            Name = $"Integration Test Product {Guid.NewGuid()}",
            Description = "Created by integration test - will be archived/deleted in TearDown",
        };
        SellableItemPriceEntity priceEntity = new SellableItemPriceEntity
        {
            SellableItemPriceID = Guid.NewGuid().ToString(),
            SellableItemID = Guid.NewGuid().ToString(),
            Amount = new Money(9.99m, CurrencyIsoCode.EUR),
        };

        // When: CreateProductInGatewayAsync is called against Stripe test-mode API
        Result<ProductGatewayCreatedResponseModel> result =
            await Adapter.CreateProductInGatewayAsync(productEntity, priceEntity, CancellationToken.None);

        // Capture IDs for TearDown cleanup (best-effort)
        if (result.IsSuccess)
        {
            CreatedProductId = result.Value.ProductGatewayID;
            CreatedPriceId = result.Value.ProductGatewayPriceID;
        }

        // Then: result must be success with non-empty gateway IDs
        Assert.That(result.IsSuccess, Is.True,
            $"Expected Stripe test-mode to succeed but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");
        Assert.That(result.Value.ProductGatewayID, Is.Not.Null.And.Not.Empty,
            "Expected a non-empty ProductGatewayID from Stripe but got empty");
        Assert.That(result.Value.ProductGatewayPriceID, Is.Not.Null.And.Not.Empty,
            "Expected a non-empty ProductGatewayPriceID from Stripe but got empty");
        Assert.That(result.Value.ProductGatewayID, Does.StartWith("prod_"),
            $"Expected ProductGatewayID to start with 'prod_' but got: '{result.Value.ProductGatewayID}'");
        Assert.That(result.Value.ProductGatewayPriceID, Does.StartWith("price_"),
            $"Expected ProductGatewayPriceID to start with 'price_' but got: '{result.Value.ProductGatewayPriceID}'");
    }
}
