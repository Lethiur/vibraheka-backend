namespace VibraHeka.Application.Common.Exceptions;

public class UserErrors
{
    /// <summary>
    /// Represents the error code indicating that a user already exists in the system.
    /// </summary>
    public const string UserAlreadyExist = "US-000";

    /// <summary>
    /// Represents the error code indicating that the provided password is invalid.
    /// </summary>
    public const string InvalidPassword = "US-001";

    /// <summary>
    /// Represents the error code indicating that the submitted form contains invalid or improperly formatted data.
    /// </summary>
    public const string InvalidForm = "US-002";

    /// <summary>
    /// Represents the error code indicating that a user was not found in the system.
    /// </summary>
    public const string UserNotFound = "US-003";

    /// <summary>
    /// Represents the error code indicating that the provided verification code is invalid or does not meet the expected criteria.
    /// </summary>
    public const string InvalidVerificationCode = "US-004";
    
    /// <summary>
    /// Represents the error code indicating that an unexpected error has occurred.
    /// </summary>
    public const string UnexpectedError = "US-005";


    /// <summary>
    /// Represents the error code indicating that the provided email address is invalid or does not meet the expected format.
    /// </summary>
    public const string InvalidEmail = "US-006";

    /// <summary>
    /// Represents the error code indicating that the provided full name is invalid or does not meet the required criteria.
    /// </summary>
    public const string InvalidFullName = "US-007";
    
    /// <summary>
    /// Represents the error code indicating that the provided confirmation code has expired.
    /// </summary>
    public const string ExpiredCode = "US-008";
    
    /// <summary>
    /// Represents the error code indicating that the provided verification code is incorrect or does not match the expected value.
    /// </summary>
    public const string WrongVerificationCode = "US-009";

    /// <summary>
    /// Represents the error code indicating that the user's account has not been confirmed.
    /// </summary>
    public const string UserNotConfirmed = "US-010";

    /// <summary>
    /// Represents the error code indicating that the provided user ID is invalid.
    /// </summary>
    public const string InvalidUserID = "US-011";

    /// <summary>
    /// Represents the error code indicating that a limit has been exceeded, such as a rate limit or resource allocation threshold.
    /// </summary>
    public const string LimitExceeded = "US-012";
    
    /// <summary>
    /// Represents the error code indicating that the user is not authorized to perform the requested operation.
    /// </summary>
    public const string NotAuthorized = "US-013";
    
    /// <summary>
    /// Represents the error code indicating that the user has exceeded the allowed number of attempts for a specific operation or action.
    /// </summary>
    public const string TooManyAttempts = "US-012";
    
    public const string EmailTooLong = "US-013";

    public const string InvalidPasswordResetToken = "US-014";

    public const string PasswordResetTokenExpired = "US-015";

    public const string PasswordResetTokenAlreadyUsed = "US-016";
}
