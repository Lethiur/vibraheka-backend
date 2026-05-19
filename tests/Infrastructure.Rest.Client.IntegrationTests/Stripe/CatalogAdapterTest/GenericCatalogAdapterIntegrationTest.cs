using Infrastructure.Rest.Client.Stripe.Adapter;
using Infrastructure.Rest.Client.Stripe.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Stripe;

namespace Infrastructure.Rest.Client.IntegrationTests.Stripe.CatalogAdapterTest;

/// <summary>
/// Base class for CatalogAdapter integration tests against the real Stripe test-mode API.
/// Reads the Stripe secret key from appsettings.Test.json and initialises StripeConfiguration.ApiKey.
/// All derived test fixtures must clean up any Stripe resources they create.
/// </summary>
public abstract class GenericCatalogAdapterIntegrationTest : TestBase
{
    protected CatalogAdapter Adapter = default!;
    protected StripeAPIClient StripeClient = default!;
    protected ProductService StripeProductService = default!;
    protected PriceService StripePriceService = default!;

    [OneTimeSetUp]
    public void CatalogOneTimeSetUp()
    {
        base.OneTimeSetUp();
        string stripeSecretKey = ReadStripeSecretKey();
        StripeConfiguration.ApiKey = stripeSecretKey;
        StripeClient = new StripeAPIClient(CreateTestLogger<StripeAPIClient>());
        Adapter = new CatalogAdapter(StripeClient, CreateTestLogger<CatalogAdapter>());
        StripeProductService = new ProductService();
        StripePriceService = new PriceService();
    }

    private static string ReadStripeSecretKey()
    {
        IConfigurationRoot config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.Test.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        string secretKey = config["Stripe:SecretKey"]
            ?? throw new InvalidOperationException("Missing Stripe:SecretKey in appsettings.Test.json.");

        if (string.IsNullOrWhiteSpace(secretKey))
            throw new InvalidOperationException("Stripe:SecretKey is empty in appsettings.Test.json.");

        return secretKey;
    }

    /// <summary>
    /// Archives a Stripe price (prices cannot be deleted, only deactivated).
    /// Best-effort: logs warning on failure without failing the test.
    /// </summary>
    protected async Task CleanupStripePrice(string priceId)
    {
        try
        {
            await StripePriceService.UpdateAsync(priceId, new PriceUpdateOptions { Active = false });
            Console.WriteLine($"Cleanup: Archived Stripe price {priceId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not archive Stripe price {priceId}: {ex.Message}");
        }
    }

    /// <summary>
    /// Deletes a Stripe product. The product must have no active prices before deletion.
    /// Best-effort: logs warning on failure without failing the test.
    /// </summary>
    protected async Task CleanupStripeProduct(string productId)
    {
        try
        {
            await StripeProductService.DeleteAsync(productId);
            Console.WriteLine($"Cleanup: Deleted Stripe product {productId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not delete Stripe product {productId}: {ex.Message}");
        }
    }
}
