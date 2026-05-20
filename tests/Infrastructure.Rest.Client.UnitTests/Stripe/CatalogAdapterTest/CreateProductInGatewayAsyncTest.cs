using System.ComponentModel;
using System.Net;
using System.Text;
using CSharpFunctionalExtensions;
using Infrastructure.Rest.Client.Stripe.Errors;
using NMoneys;
using VibraHeka.Application.Catalog.Models;
using VibraHeka.Domain.Catalog.Entities;

namespace Infrastructure.Rest.Client.UnitTests.Stripe.CatalogAdapterTest;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class CreateProductInGatewayAsyncTest : GenericCatalogAdapterTest
{
    [Test]
    [DisplayName("Should return success with mapped ProductGatewayID and ProductGatewayPriceID when Stripe creates product and price")]
    public async Task ShouldReturnSuccessWithMappedGatewayIdsWhenStripeCreatesProductAndPrice()
    {
        // Given: Stripe returns a successful product response followed by a successful price response
        string expectedProductId = "prod_unit_test_abc";
        string expectedPriceId = "price_unit_test_xyz";

        FakeHandler.EnqueueResponse(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                BuildStripeProductSuccessJson(expectedProductId),
                Encoding.UTF8,
                "application/json"),
        });
        FakeHandler.EnqueueResponse(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                BuildStripePriceSuccessJson(expectedPriceId, expectedProductId),
                Encoding.UTF8,
                "application/json"),
        });

        ProductEntity productEntity = new ProductEntity
        {
            ProductID = Guid.NewGuid().ToString(),
            Name = "Meditacion Matutina",
            Description = "Sesion de meditacion para iniciar el dia",
        };
        SellableItemPriceEntity priceEntity = new SellableItemPriceEntity
        {
            SellableItemPriceID = Guid.NewGuid().ToString(),
            SellableItemID = Guid.NewGuid().ToString(),
            Amount = new Money(9.99m, CurrencyIsoCode.EUR),
        };

        // When: CreateProductInGatewayAsync is called
        Result<ProductGatewayCreatedResponseModel> result =
            await Adapter.CreateProductInGatewayAsync(productEntity, priceEntity, CancellationToken.None);

        // Then: result should be success with the Stripe IDs correctly mapped
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");
        Assert.That(result.Value.ProductGatewayID, Is.EqualTo(expectedProductId),
            $"Expected ProductGatewayID '{expectedProductId}' but got '{result.Value.ProductGatewayID}'");
        Assert.That(result.Value.ProductGatewayPriceID, Is.EqualTo(expectedPriceId),
            $"Expected ProductGatewayPriceID '{expectedPriceId}' but got '{result.Value.ProductGatewayPriceID}'");
        Assert.That(result.Value.ProductGatewayID, Is.Not.Null.And.Not.Empty,
            "ProductGatewayID must not be empty when Stripe succeeds");
        Assert.That(result.Value.ProductGatewayPriceID, Is.Not.Null.And.Not.Empty,
            "ProductGatewayPriceID must not be empty when Stripe succeeds");
    }

    [Test]
    [DisplayName("Should return S-003 failure when Stripe returns an error response (maps to constant, no free strings)")]
    public async Task ShouldReturnS003FailureWhenStripeReturnsErrorResponse()
    {
        // Given: Stripe returns a 500 error on product creation
        FakeHandler.EnqueueResponse(new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent(
                BuildStripeErrorJson(),
                Encoding.UTF8,
                "application/json"),
        });

        ProductEntity productEntity = new ProductEntity
        {
            ProductID = Guid.NewGuid().ToString(),
            Name = "Test Product",
            Description = "Test",
        };
        SellableItemPriceEntity priceEntity = new SellableItemPriceEntity
        {
            SellableItemPriceID = Guid.NewGuid().ToString(),
            SellableItemID = Guid.NewGuid().ToString(),
            Amount = new Money(9.99m, CurrencyIsoCode.EUR),
        };

        // When: CreateProductInGatewayAsync is called
        Result<ProductGatewayCreatedResponseModel> result =
            await Adapter.CreateProductInGatewayAsync(productEntity, priceEntity, CancellationToken.None);

        // Then: result should be failure with S-003 constant (no free-text error codes)
        Assert.That(result.IsFailure, Is.True,
            $"Expected failure when Stripe returns error but got success with ProductGatewayID: '{(result.IsSuccess ? result.Value.ProductGatewayID : "N/A")}'");
        Assert.That(result.Error, Is.EqualTo(StripeErrors.FailedToCreateProductAndPrice),
            $"Expected error '{StripeErrors.FailedToCreateProductAndPrice}' (S-003) but got '{result.Error}'");
        Assert.That(result.Error, Does.Not.Contain(" "),
            $"Error code must be a constant without free text, but got: '{result.Error}'");
    }
}
