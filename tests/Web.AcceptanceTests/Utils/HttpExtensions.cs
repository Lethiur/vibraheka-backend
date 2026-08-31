using static System.Text.Json.JsonSerializer;

namespace VibraHeka.Web.AcceptanceTests.Utils;

public static class HttpExtensions
{
    public static async Task<T> ParseContentAsync<T>(this HttpResponseMessage response)
    {
        string content = await response.Content.ReadAsStringAsync();
        return Deserialize<T>(content) ?? throw new InvalidOperationException("Failed to deserialize response content.");
    }
}
