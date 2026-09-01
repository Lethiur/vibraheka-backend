using System.Net;
using System.Text.Json;
using NUnit.Framework;
using VibraHeka.Web.Authentication;
using static System.Text.Json.JsonSerializer;

namespace VibraHeka.Web.AcceptanceTests.Utils;

public static class HttpExtensions
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    public static async Task<T> ParseContentAsync<T>(this HttpResponseMessage response)
    {
        string content = await response.Content.ReadAsStringAsync();
        return Deserialize<T>(content, Options) ?? throw new InvalidOperationException("Failed to deserialize response content.");
    }
    
    public static async Task AssertBadRequestWithError(this HttpResponseMessage response, string expectedErrorCode)
    {
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        BadRequestResponse entity = await response.ParseContentAsync<BadRequestResponse>();
        Assert.That(entity.ErrorCode, Does.Contain(expectedErrorCode));
    }
}
