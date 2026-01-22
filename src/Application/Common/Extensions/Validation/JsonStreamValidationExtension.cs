using System.Text.Json;

namespace VibraHeka.Application.Common.Extensions.Validation;

/// <summary>
/// Provides an extension method to validate if a stream contains valid JSON data.
/// </summary>
public static class JsonStreamValidationExtension
{
    /// <summary>
    /// Validates whether the provided stream contains valid JSON data.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder that defines validation rules for the property being checked.</param>
    /// <returns>
    /// An instance of <see cref="IRuleBuilderOptions{T, Stream}"/> configured to validate if the stream contains valid JSON.
    /// </returns>
    public static IRuleBuilderOptions<T, Stream> ValidJsonStream<T>(this IRuleBuilder<T, Stream> ruleBuilder)
    {
        return ruleBuilder.Must((stream) =>
        {
            try
            {
                stream.Position = 0;
                using JsonDocument document =
                    JsonDocument.Parse(
                        stream
                    );
            }
            catch (JsonException)
            {
                return false;
            }
            finally
            {
                stream.Position = 0;
            }

            return true;
        });
    }
}
