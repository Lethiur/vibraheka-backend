namespace VibraHeka.Infrastructure.Exceptions;

public class UserException : ApplicationException
{
    /// <summary>
    /// Represents the error code indicating that a user already exists in the system.
    /// </summary>
    public const string UserAlreadyExist = "E-000";

    /// <summary>
    /// Represents the error code indicating that the provided password is invalid.
    /// </summary>
    public const string InvalidPassword = "E-001";

    /// <summary>
    /// Represents the error code indicating that the submitted form contains invalid or improperly formatted data.
    /// </summary>
    public const string InvalidForm = "E-002";

    /// <summary>
    /// Represents the error code indicating that a user was not found in the system.
    /// </summary>
    public const string UserNotFound = "E-003";

    /// <summary>
    /// Represents the error code indicating that the provided verification code is invalid or does not meet the expected criteria.
    /// </summary>
    public const string InvalidVerificationCode = "E-004";
    
    public const string InvalidEmail = "E-006";
    public const string InvalidFullName = "E-007";

    /// <summary>
    /// Represents the error code indicating that an unexpected error has occurred.
    /// </summary>
    public const string UnexpectedError = "E-005";
    
    public UserException(string errorCode, string errorMessage) : base(errorCode, errorMessage)
    {
    }
}
