using System.Text.Json.Serialization;
using NMoneys;
using VibraHeka.Domain.Catalog.Enums;

namespace VibraHeka.Web.Entities;

public class CreateProductPriceRequest
{
  
    /// <summary>
    /// A detailed description of the product.
    /// </summary>
    public string SellableItemID { get; set; } = string.Empty;

    public PriceKind Kind { get; set; }
    
    public BillingInterval? BillingInterval { get; set; }

    /// <summary>
    /// The unit price of the product.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// The ISO 4217 currency code for the product price (e.g. EUR, USD).
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CurrencyIsoCode Currency { get; set; }
}
