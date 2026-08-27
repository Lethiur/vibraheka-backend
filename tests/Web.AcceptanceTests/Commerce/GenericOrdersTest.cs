using System.Net.Http.Headers;
using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using NMoneys;
using VibraHeka.Application.Catalog.Models;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Enums;
using VibraHeka.Domain.Catalog.Ports.Out;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Models.Results;
using VibraHeka.Web.AcceptanceTests.Generic;
using VibraHeka.Web.Entities;

namespace VibraHeka.Web.AcceptanceTests.Commerce;

/// <summary>
/// Base class for Orders endpoint (POST api/v1/orders) acceptance tests.
/// Authentication helpers are inherited from <see cref="GenericAcceptanceTest{TAppClass}"/>.
/// </summary>
public abstract class GenericOrdersTest : GenericAcceptanceTest<VibraHekaProgram>
{
    protected const string OrdersEndpoint = "/api/v1/orders";
    private const string SeedRecordingEndpoint = "/api/v1/catalog/recordings";

    protected CreateOrderRequest BuildValidRequest(
        string sellableItemId,
        string sellableItemPriceId,
        string? idempotencyKey = null) =>
        new()
        {
            IdempotencyKey = idempotencyKey ?? Guid.NewGuid().ToString(),
            OrderLines =
            [
                new CreateOrderLineRequest
                {
                    SellableItemID = sellableItemId,
                    SellableItemPriceID = sellableItemPriceId,
                    Quantity = 1
                }
            ]
        };

    protected CreateOrderRequest BuildValidRequest(string? idempotencyKey = null) =>
        new()
        {
            IdempotencyKey = idempotencyKey ?? Guid.NewGuid().ToString(),
            OrderLines =
            [
                new CreateOrderLineRequest
                {
                    SellableItemID = "item-acceptance-001",
                    SellableItemPriceID = "price-acceptance-001",
                    Quantity = 1
                }
            ]
        };

    protected CreateOrderRequest BuildRequestWithNoLines(string? idempotencyKey = null) =>
        new()
        {
            IdempotencyKey = idempotencyKey ?? Guid.NewGuid().ToString(),
            OrderLines = []
        };

    protected CreateOrderRequest BuildRequestWithEmptyIdempotencyKey() =>
        new()
        {
            IdempotencyKey = string.Empty,
            OrderLines =
            [
                new CreateOrderLineRequest
                {
                    SellableItemID = "item-acceptance-001",
                    SellableItemPriceID = "price-acceptance-001",
                    Quantity = 1
                }
            ]
        };

    protected CreateOrderRequest BuildRequestWithEmptySellableItemId() =>
        new()
        {
            IdempotencyKey = Guid.NewGuid().ToString(),
            OrderLines =
            [
                new CreateOrderLineRequest
                {
                    SellableItemID = string.Empty,
                    SellableItemPriceID = "price-acceptance-001",
                    Quantity = 1
                }
            ]
        };

    protected CreateOrderRequest BuildRequestWithEmptySellableItemPriceId() =>
        new()
        {
            IdempotencyKey = Guid.NewGuid().ToString(),
            OrderLines =
            [
                new CreateOrderLineRequest
                {
                    SellableItemID = "item-acceptance-001",
                    SellableItemPriceID = string.Empty,
                    Quantity = 1
                }
            ]
        };

    protected CreateOrderRequest BuildRequestWithZeroQuantity() =>
        new()
        {
            IdempotencyKey = Guid.NewGuid().ToString(),
            OrderLines =
            [
                new CreateOrderLineRequest
                {
                    SellableItemID = "item-acceptance-001",
                    SellableItemPriceID = "price-acceptance-001",
                    Quantity = 0
                }
            ]
        };

    /// <summary>
    /// Seeds a product via the catalog endpoint using a temporary admin user.
    /// Returns the IDs required to build a valid order request:
    ///   - <c>SellableItemId</c>: the ProductID used as the SellableItem reference in order lines.
    ///   - <c>SellableItemPriceId</c>: the actual SellableItemPriceID hash key.
    /// Resets <c>Client.DefaultRequestHeaders.Authorization</c> to null after seeding.
    /// </summary>
    protected async Task<(string SellableItemId, string SellableItemPriceId)> SeedCatalogProductAsync()
    {
        // Create and authenticate an admin user for catalog seeding
        string adminEmail = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Person.FullName, adminEmail, ThePassword);
        AuthenticationResult adminAuth = await AuthenticateUser(adminEmail, ThePassword);

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", adminAuth.AccessToken);

        // Create the product through the catalog API
        HttpResponseMessage catalogResponse = await Client.PostAsJsonAsync(
            SeedRecordingEndpoint,
            new UploadRecordingRequest
            {
                Name = "Acceptance Test Product",
                Description = "Product seeded for order acceptance test",
                Price = 9.99m,
                CurrencyCode = CurrencyIsoCode.EUR,
            });
        catalogResponse.EnsureSuccessStatusCode();

        // Reset auth so the calling test can set its own token
        Client.DefaultRequestHeaders.Authorization = null;

        // Extract the productId (= SellableItem ReferenceID = what order handler uses as SellableItemID)
        ResponseEntity catalogEntity = await catalogResponse.GetAsResponseEntityAndContentAs<AddRecordingResult>();
        AddRecordingResult productId = catalogEntity.GetContentAs<AddRecordingResult>()!;

        // Retrieve the SellableItem via domain port (keyed by ReferenceID = productId)
        ISellableItemPort sellableItemPort = GetObjectFromFactory<ISellableItemPort>();
        Result<SellableItemEntity> itemResult =
            await sellableItemPort.GetSellableItemByReferenceAsync(productId.RecordingId, CancellationToken.None);
        SellableItemEntity sellableItemEntity = itemResult.Value;

        // Retrieve the price via domain port (one-time price keyed by SellableItemID GSI)
        ISellableItemPricePort sellableItemPricePort = GetObjectFromFactory<ISellableItemPricePort>();
        Result<SellableItemPriceEntity> priceResult =
            await sellableItemPricePort.GetSellableItemPriceAndKindAsync(
                sellableItemEntity.SellableItemID, PriceKind.OneTime, CancellationToken.None);
        SellableItemPriceEntity priceEntity = priceResult.Value;

        // The order line uses productId as SellableItemID (adapter resolves via ReferenceID index)
        return (sellableItemEntity.SellableItemID, priceEntity.SellableItemPriceID);
    }
}
