using System.Text.Json.Serialization;
using NMoneys;

namespace VibraHeka.Web.Entities;

/// <summary>
/// Represents the payload required to create a new product in the catalog.
/// </summary>
public class CreateProductRequest
{
    /// <summary>
    /// The display name of the product.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// A detailed description of the product.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The unit price of the product.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// The ISO 4217 currency code for the product price (e.g. EUR, USD).
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CurrencyIsoCode CurrencyCode { get; set; }
}

