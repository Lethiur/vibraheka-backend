namespace VibraHeka.Infrastructure.Exceptions;

/// <summary>
/// Represents error codes used in the infrastructure file management process.
/// These error codes are used to classify and identify specific issues related
/// to file management within the infrastructure layer.
/// </summary>
public static class InfrastructureFileManagementErrors
{
    /// <summary>
    /// Error code indicating that an invalid key was provided during
    /// operations related to file management in the infrastructure layer.
    /// Typically used when the key is null, empty, or does not meet
    /// the required specifications for a file identifier.
    /// </summary>
    public static readonly string InvalidKey = "IFM-000";

    /// <summary>
    /// Error code indicating that an invalid expiry date or duration was provided
    /// during operations related to file management in the infrastructure layer.
    /// This is typically used when the specified expiration value is negative or otherwise invalid.
    /// </summary>
    public static readonly string InvalidExpiryDate = "IFM-001";

    /// <summary>
    /// Error code indicating that an invalid hash value was provided during
    /// operations related to file management in the infrastructure layer.
    /// This typically occurs when the hash is null, empty, or does not meet
    /// the expected format or standards for a valid MD5 hash.
    /// </summary>
    public static readonly string InvalidHash = "IFM-002";
        
}
