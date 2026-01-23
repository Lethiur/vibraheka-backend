namespace VibraHeka.Application.Common.Extensions.Validation;

/// <summary>
/// Provides an extension method to validate if a stream contains image or video content.
/// </summary>
public static class MediaStreamValidationExtension
{
    private static readonly byte[] JpegPrefix = { 0xFF, 0xD8, 0xFF };
    private static readonly byte[] PngSignature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
    private static readonly byte[] Gif87aSignature = { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 };
    private static readonly byte[] Gif89aSignature = { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 };
    private static readonly byte[] BmpSignature = { 0x42, 0x4D };
    private static readonly byte[] RiffSignature = { 0x52, 0x49, 0x46, 0x46 };
    private static readonly byte[] WebpSignature = { 0x57, 0x45, 0x42, 0x50 };
    private static readonly byte[] AviSignature = { 0x41, 0x56, 0x49, 0x20 };
    private static readonly byte[] FtypSignature = { 0x66, 0x74, 0x79, 0x70 };
    private static readonly byte[] MkvSignature = { 0x1A, 0x45, 0xDF, 0xA3 };
    private static readonly byte[] OggSignature = { 0x4F, 0x67, 0x67, 0x53 };

    /// <summary>
    /// Validates whether the provided stream starts with known image or video signatures.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder that defines validation rules for the property being checked.</param>
    /// <returns>
    /// An instance of <see cref="IRuleBuilderOptions{T, Stream}"/> configured to validate if the stream is an image or video.
    /// </returns>
    public static IRuleBuilderOptions<T, Stream> ValidImageOrVideoStream<T>(this IRuleBuilder<T, Stream> ruleBuilder)
    {
        return ruleBuilder.Must(stream =>
        {
            if (stream is not { CanSeek: true })
            {
                return false;
            }

            long originalPosition = stream.Position;
            try
            {
                stream.Position = 0;
                byte[] buffer = new byte[64];
                int read = stream.Read(buffer, 0, buffer.Length);
                if (read <= 0)
                {
                    return false;
                }

                return IsImage(buffer, read) || IsVideo(buffer, read);
            }
            finally
            {
                stream.Position = originalPosition;
            }
        });
    }

    private static bool IsImage(byte[] buffer, int count)
    {
        return HasPrefix(buffer, count, JpegPrefix)
               || HasPrefix(buffer, count, PngSignature)
               || HasPrefix(buffer, count, Gif87aSignature)
               || HasPrefix(buffer, count, Gif89aSignature)
               || HasPrefix(buffer, count, BmpSignature)
               || (HasPrefix(buffer, count, RiffSignature) && HasAtOffset(buffer, count, WebpSignature, 8));
    }

    private static bool IsVideo(byte[] buffer, int count)
    {
        return (HasPrefix(buffer, count, RiffSignature) && HasAtOffset(buffer, count, AviSignature, 8))
               || HasAtOffset(buffer, count, FtypSignature, 4)
               || HasPrefix(buffer, count, MkvSignature)
               || HasPrefix(buffer, count, OggSignature);
    }

    private static bool HasPrefix(byte[] buffer, int count, byte[] prefix)
    {
        if (count < prefix.Length)
        {
            return false;
        }

        return !prefix.Where((t, i) => buffer[i] != t).Any();
    }

    private static bool HasAtOffset(byte[] buffer, int count, byte[] signature, int offset)
    {
        int requiredLength = offset + signature.Length;
        if (count < requiredLength)
        {
            return false;
        }

        return !signature.Where((t, i) => buffer[offset + i] != t).Any();
    }
}
