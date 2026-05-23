using CSharpFunctionalExtensions;
using Infrastructure.Rest.Client.Stripe.Client;
using Infrastructure.Rest.Client.Stripe.Models;
using Microsoft.Extensions.Logging;
using VibraHeka.Application.Catalog.Models;
using VibraHeka.Application.Catalog.Ports.Out;
using VibraHeka.Domain.Catalog.Entities;

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

        Logger.LogInformation("Creating product and price in Stripe gateway {ProductID}", productEntity.ProductID);
        CreateProductAndPriceRequest request = new CreateProductAndPriceRequest()
        {
            Name = productEntity.Name,
            Description = productEntity.Description,
            Currency = priceEntity.Amount.CurrencyCode.ToString().ToLowerInvariant(),
            PriceInCents = priceEntity.Amount.MinorIntegralAmount,
            Metadata = new Dictionary<string, string>()
            {
                { "ProductID", productEntity.ProductID }, { "SellableItemID", priceEntity.SellableItemID }
            },
        };

        return Client.CreateProductAndPriceAsync(request, cancellationToken).Map(response => new ProductGatewayCreatedResponseModel()
        {
            ProductGatewayID = response.ProductID,
            ProductGatewayPriceID = response.PriceID
        });
    }
}
