namespace VibraHeka.Application.Common.Models.Results;

/// <summary>
/// Represents the result of a user registration process.
/// Contains information about the registered user's ID and whether further confirmation is needed.
/// </summary>
public record UserRegistrationResult(string UserId, bool needsConfirmation);
