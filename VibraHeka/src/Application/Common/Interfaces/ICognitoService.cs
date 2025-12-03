using CSharpFunctionalExtensions;

namespace VibraHeka.Application.Common.Interfaces;

/// <summary>
/// Defines the contract for interacting with the AWS Cognito authentication service.
/// </summary>
public interface ICognitoService
{
    /// <summary>
    /// Asynchronously registers a new user with the provided email, password, and full name using AWS Cognito.
    /// </summary>
    /// <param name="email">The email address of the user to be registered.</param>
    /// <param name="password">The password for the user account.</param>
    /// <param name="fullName">The full name of the user.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a <see cref="Result"/> where T is a string representing the unique identifier
    /// of the registered user in AWS Cognito, if successful.
    /// </returns>
    Task<Result<string>> RegisterUserAsync(string email, string password, string fullName);
}
