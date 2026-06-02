using Infrastructure.Rest.Client.Stripe.Adapter;
using Infrastructure.Rest.Client.Stripe.Client;
using Microsoft.Extensions.Logging.Abstractions;
using Stripe;
using VibraHeka.Infrastructure.Rest.Client.UnitTests.Helpers;

namespace VibraHeka.Infrastructure.Rest.Client.UnitTests.Stripe.CatalogAdapterTest;

/// <summary>
/// Base class for CatalogAdapter unit tests.
/// Uses FakeHttpMessageHandler injected via StripeConfiguration.StripeClient to intercept
/// all Stripe SDK HTTP calls without making real network requests.
/// </summary>
public abstract class GenericCatalogAdapterTest
{
    protected FakeHttpMessageHandler FakeHandler = default!;
    protected CatalogAdapter Adapter = default!;
    protected StripeAPIClient StripeClient = default!;

    [SetUp]
    public virtual void SetUp()
    {
        FakeHandler = new FakeHttpMessageHandler();
        SystemNetHttpClient fakeStripeHttpClient = new SystemNetHttpClient(new HttpClient(FakeHandler));
        StripeConfiguration.StripeClient = new StripeClient(
            "sk_test_unit_test_fake_key_catalog",
            httpClient: fakeStripeHttpClient);
        StripeClient = new StripeAPIClient(NullLogger<StripeAPIClient>.Instance);
        Adapter = new CatalogAdapter(StripeClient, NullLogger<CatalogAdapter>.Instance);
    }

    [TearDown]
    public virtual void TearDown()
    {
        StripeConfiguration.StripeClient = null;
        FakeHandler.Dispose();
    }

    protected static string BuildStripeProductSuccessJson(string productId = "prod_unit_test_001") =>
        $@"{{""id"":""{productId}"",""object"":""product"",""name"":""Test Product"",""active"":true,""created"":1700000000,""updated"":1700000000,""livemode"":false,""description"":null,""images"":[],""metadata"":{{}}}}";

    protected static string BuildStripePriceSuccessJson(
        string priceId = "price_unit_test_001",
        string productId = "prod_unit_test_001") =>
        $@"{{""id"":""{priceId}"",""object"":""price"",""active"":true,""billing_scheme"":""per_unit"",""created"":1700000000,""currency"":""eur"",""livemode"":false,""metadata"":{{}},""product"":""{productId}"",""type"":""one_time"",""unit_amount"":999,""unit_amount_decimal"":""999""}}";

    protected static string BuildStripeErrorJson() =>
        @"{""error"":{""type"":""api_error"",""message"":""Stripe service unavailable""}}";
}
