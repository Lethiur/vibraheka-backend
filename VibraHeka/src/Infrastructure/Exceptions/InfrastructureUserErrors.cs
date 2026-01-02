namespace VibraHeka.Infrastructure.Exceptions;

/// <summary>
/// Provides a centralized collection of error codes related to user-specific errors within the infrastructure layer.
/// This class is designed to simplify error management and improve code maintainability by standardizing
/// the error codes across the infrastructure layer.
/// </summary>
public class InfrastructureUserErrors
{
    /// <summary>
    /// Represents the error code associated with a user not being found within the system.
    /// This value is used to standardize the handling and identification of such errors
    /// in the infrastructure layer.
    /// </summary>
    public const string UserNotFound = "I-000";
}
