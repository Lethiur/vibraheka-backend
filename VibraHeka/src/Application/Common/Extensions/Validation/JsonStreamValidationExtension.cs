using System.Text.Json;

namespace VibraHeka.Application.Common.Extensions.Validation;

public static class JsonStreamValidationExtension
{
    public static IRuleBuilderOptions<T, Stream> ValidJsonStream<T>(this IRuleBuilder<T, Stream> ruleBuilder)
    {
        return ruleBuilder.MustAsync(async (stream, ct) =>
        {
            try
            {
                stream.Position = 0;
                using JsonDocument document = await JsonDocument.ParseAsync(
                    stream,
                    cancellationToken: ct
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
