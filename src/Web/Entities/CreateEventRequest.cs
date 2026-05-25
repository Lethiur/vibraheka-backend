using System.Text.Json.Serialization;
using NMoneys;

namespace VibraHeka.Web.Entities;

public class CreateEventRequest
{
    public string EventName { get; set; } = string.Empty;
    public string EventDescription { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public int Duration { get; set; }
    public string EventTimezone { get; set; } = string.Empty;
    public decimal Price { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CurrencyIsoCode CurrencyCode { get; set; }
}
