using CSharpFunctionalExtensions;
using Infrastructure.Rest.Client.Stripe.Client;
using Infrastructure.Rest.Client.Stripe.Enums;
using Infrastructure.Rest.Client.Stripe.Models;
using Microsoft.Extensions.Logging;
using VibraHeka.Application.Catalog.Models;
using VibraHeka.Application.Catalog.Ports.Out;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Enums;

namespace Infrastructure.Rest.Client.Stripe.Adapter;

/// <summary>
/// Adapter class that integrates with the Stripe API client to facilitate
/// the creation of products and pricing information in the Stripe gateway.
/// This class acts as an implementation of the <see cref="IProductCreationWritePort"/> interface.
/// </summary>
public class CatalogAdapter(StripeAPIClient Client, ILogger<CatalogAdapter> Logger) : IProductCreationWritePort
{
    /// <summary>
    /// Creates a product and its associated pricing information in the payment gateway (Stripe).
    /// Maps the provided product and price entities to the request model
    /// and sends the data to the Stripe API via the Stripe client.
    /// </summary>
    /// <param name="productEntity">
    /// The domain entity representing the product, containing details such as the product's ID, name, and description.
    /// </param>
    /// <param name="priceEntity">
    /// The domain entity representing the product's pricing details, including the amount and associated IDs.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to observe while awaiting the operation, enabling cancellation if required.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. When completed, contains a result object
    /// with a response model that includes gateway-specific IDs for the created product and price.
    /// </returns>
    public Task<Result<ProductGatewayCreatedResponseModel>> CreateProductInGatewayAsync(ProductEntity productEntity,
        SellableItemPriceEntity priceEntity,
        CancellationToken cancellationToken)
    {
        Logger.LogInformation("Creating product and price in Stripe gateway {ProductID}", productEntity.ID);

        PaymentRecurringOptions? options = priceEntity.BillingInterval switch
        {
            BillingInterval.Monthly => PaymentRecurringOptions.Monthly,
            BillingInterval.Yearly => PaymentRecurringOptions.Yearly,
            _ => null
        };

        CreateProductAndPriceRequest request = new()
        {
            Name = productEntity.Name,
            Description = productEntity.Description,
            Currency = priceEntity.Amount.CurrencyCode.ToString().ToLowerInvariant(),
            PriceInCents = priceEntity.Amount.MinorIntegralAmount,
            PaymentRecurringOptions = options,
            Metadata = new Dictionary<string, string>
            {
                { "ProductID", productEntity.ID }, { "SellableItemID", priceEntity.SellableItemID }
            },
        };

        return Client.CreateProductAndPriceAsync(request, cancellationToken).Map(response =>
            new ProductGatewayCreatedResponseModel()
            {
                ProductGatewayID = response.ProductID, ProductGatewayPriceID = response.PriceID
            });
    }

    /// <summary>
    /// Associates a sellable item's pricing details with a product in the payment gateway (Stripe).
    /// Maps the provided sellable item price entity to the request model
    /// and communicates with the Stripe API to update the product's pricing information.
    /// </summary>
    /// <param name="price">
    /// The domain entity representing the sellable item's pricing information, including details
    /// such as the price amount, currency, and additional identifiers.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to observe while awaiting the operation, enabling cancellation if required.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. When completed, contains a result object
    /// with a response model that includes gateway-specific identifiers for the updated product.
    /// </returns>
    public Task<Result<ProductGatewayCreatedResponseModel>> AddSellableItemPriceToProduct(SellableItemPriceEntity price,
        CancellationToken cancellationToken)
    {
        Logger.LogInformation("Adding price to product in Stripe gateway for SellableItemID {SellableItemID}",
            price.SellableItemID);
        CreatePriceRequest request = new()
        {
            Currency = price.Amount.CurrencyCode.ToString().ToLowerInvariant(),
            PriceInCents = price.Amount.MinorIntegralAmount,
            ProductID = price.ExternalProductID,
            Metadata = new Dictionary<string, string>
            {
                { "SellableItemID", price.SellableItemID }, { "SellableItemPriceID", price.SellableItemPriceID }
            },
        };

        return Client.AddPriceToProduct(request, cancellationToken).Map(response =>
            new ProductGatewayCreatedResponseModel()
            {
                ProductGatewayID = price.ExternalProductID, ProductGatewayPriceID = response
            });
    }
}
