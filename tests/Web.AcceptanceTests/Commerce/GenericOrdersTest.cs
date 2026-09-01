using System.Net.Http.Headers;
using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using NMoneys;
using VibraHeka.Application.Catalog.Models;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Enums;
using VibraHeka.Domain.Catalog.Ports.Out;
using VibraHeka.Domain.Models.Results;
using VibraHeka.Web.AcceptanceTests.Catalog;
using VibraHeka.Web.AcceptanceTests.Generic;
using VibraHeka.Web.AcceptanceTests.Utils;
using VibraHeka.Web.Authentication;
using VibraHeka.Web.Catalog.Orders.Controllers;
using VibraHeka.Web.Catalog.Recordings.Controllers;

namespace VibraHeka.Web.AcceptanceTests.Commerce;

/// <summary>
/// Base class for Orders endpoint (POST api/v1/orders) acceptance tests.
/// Authentication helpers are inherited from <see cref="GenericAcceptanceTest{TAppClass}"/>.
/// </summary>
public abstract class GenericOrdersTest : GenericAcceptanceTest<VibraHekaProgram>
{
    protected const string OrdersEndpoint = "/api/v1/orders";
    private const string SeedRecordingEndpoint = "/api/v1/catalog/recordings/admin";

    protected CreateOrderRequest BuildValidRequest(
        Guid sellableItemId,
        Guid sellableItemPriceId,
        Guid idempotencyKey) =>
        new()
        {
            Order = new()
            {
                IdempotencyKey = idempotencyKey,
                Lines =
                [
                    new() { ProductId = sellableItemId, PriceId = sellableItemPriceId, Quantity = 1 }
                ]
            }
        };

    protected CreateOrderRequest BuildValidRequest(Guid idempotencyKey) =>
        new()
        {
            Order = new()
            {
                IdempotencyKey = idempotencyKey,
                Lines =
                [
                    new() { ProductId = Guid.NewGuid(), PriceId = Guid.NewGuid(), Quantity = 1 }
                ]
            }
        };

    protected CreateOrderRequest BuildRequestWithNoLines(Guid idempotencyKey) =>
        new() { Order = new() { IdempotencyKey = idempotencyKey, Lines = [] } };

    protected CreateOrderRequest BuildRequestWithEmptyIdempotencyKey() =>
        new()
        {
            Order = new()
            {
                IdempotencyKey = Guid.Empty,
                Lines =
                [
                    new() { ProductId = Guid.NewGuid(), PriceId = Guid.NewGuid(), Quantity = 1 }
                ]
            }
        };

    protected CreateOrderRequest BuildRequestWithEmptySellableItemId() =>
        new()
        {
            Order = new()
            {
                IdempotencyKey = Guid.NewGuid(),
                Lines =
                [
                    new() { ProductId = Guid.Empty, PriceId = Guid.NewGuid(), Quantity = 1 }
                ]
            }
        };

    protected CreateOrderRequest BuildRequestWithEmptySellableItemPriceId() =>
        new()
        {
            Order = new()
            {
                IdempotencyKey = Guid.NewGuid(),
                Lines =
                [
                    new() { ProductId = Guid.NewGuid(), PriceId = Guid.Empty, Quantity = 1 }
                ]
            }
        };

    protected CreateOrderRequest BuildRequestWithZeroQuantity() =>
        new()
        {
            Order = new()
            {
                IdempotencyKey = Guid.NewGuid(),
                Lines =
                [
                    new() { ProductId = Guid.Empty, PriceId = Guid.NewGuid(), Quantity = 0 }
                ]
            }
        };

    /// <summary>
    /// Seeds a product via the catalog endpoint using a temporary admin user.
    /// Returns the IDs required to build a valid order request:
    ///   - <c>SellableItemId</c>: the ProductID used as the SellableItem reference in order lines.
    ///   - <c>SellableItemPriceId</c>: the actual SellableItemPriceID hash key.
    /// Resets <c>Client.DefaultRequestHeaders.Authorization</c> to null after seeding.
    /// </summary>
    protected async Task<(Guid SellableItemId, Guid SellableItemPriceId)> SeedCatalogProductAsync()
    {
        // Create and authenticate an admin user for catalog seeding
        string adminEmail = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Person.FullName, adminEmail, ThePassword);
        AuthenticateUserResponse adminAuth = await AuthenticateUser(adminEmail, ThePassword);

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", adminAuth.AccessToken);

        // Create the product through the catalog API
        HttpResponseMessage catalogResponse = await Client.PutAsJsonAsync(
            SeedRecordingEndpoint,
            new CreateRecordingRequest()
            {
                Name = "Acceptance Test Product",
                Description = "Product seeded for order acceptance test",
                Price = 9.99m,
                Currency = nameof(CurrencyIsoCode.EUR),
            });
        catalogResponse.EnsureSuccessStatusCode();

        // Reset auth so the calling test can set its own token
        Client.DefaultRequestHeaders.Authorization = null;
        CreateRecordingResponse productId = await catalogResponse.ParseContentAsync<CreateRecordingResponse>();

        // Retrieve the SellableItem via domain port (keyed by ReferenceID = productId)
        ISellableItemPort sellableItemPort = GetObjectFromFactory<ISellableItemPort>();
        Result<SellableItemEntity> itemResult =
            await sellableItemPort.GetSellableItemByReferenceAsync(productId.Id.ToString(), CancellationToken.None);
        SellableItemEntity sellableItemEntity = itemResult.Value;

        // Retrieve the price via domain port (one-time price keyed by SellableItemID GSI)
        ISellableItemPricePort sellableItemPricePort = GetObjectFromFactory<ISellableItemPricePort>();
        Result<SellableItemPriceEntity> priceResult =
            await sellableItemPricePort.GetSellableItemPriceAndKindAsync(
                sellableItemEntity.SellableItemID, PriceKind.OneTime, CancellationToken.None);
        SellableItemPriceEntity priceEntity = priceResult.Value;

        // The order line uses productId as SellableItemID (adapter resolves via ReferenceID index)
        return (Guid.Parse(sellableItemEntity.SellableItemID), Guid.Parse(priceEntity.SellableItemPriceID));
    }
}
