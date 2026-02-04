using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Models.Results;

namespace VibraHeka.Domain.Common.Interfaces.User;

/// <summary>
/// Defines the contract for interacting with the AWS Cognito authentication service.
/// </summary>
public interface IUserService
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

    /// <summary>
    /// Asynchronously confirms a user's registration in AWS Cognito using the provided email and confirmation code.
    /// </summary>
    /// <param name="email">The email address of the user whose registration is being confirmed.</param>
    /// <param name="confirmationCode">The confirmation code sent to the user's email address during the registration process.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a <see cref="Result"/> where T is <see cref="Unit"/>, indicating the success or failure of the confirmation process.
    /// </returns>
    Task<Result<Unit>> ConfirmUserAsync(string email, string confirmationCode);

    /// <summary>
    /// Authenticates a user by validating the provided email and password against the Cognito user pool.
    /// </summary>
    /// <param name="email">The email address of the user attempting to authenticate.</param>
    /// <param name="password">The password associated with the user's account.</param>
    /// <returns>A result containing an <see cref="AuthenticationResult"/> with the user's ID, access token, and refresh token upon successful authentication, or an error in case of failure.</returns>
    public Task<Result<AuthenticationResult>> AuthenticateUserAsync(string email, string password);

    /// <summary>
    /// Asynchronously resends the verification code to the specified email address through AWS Cognito.
    /// </summary>
    /// <param name="email">The email address to which the verification code will be resent.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a <see cref="Result"/> where T is <see cref="Unit"/>, indicating the outcome of the resend operation.
    /// </returns>
    Task<Result<Unit>> ResendVerificationCodeAsync(string email);

    /// <summary>
    /// Asynchronously retrieves the unique identifier of a user from AWS Cognito based on the provided email address.
    /// </summary>
    /// <param name="email">The email address of the user whose unique identifier is being retrieved.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a <see cref="Result"/> where T is a string representing the unique identifier of the user, if found.
    /// </returns>
    Task<Result<string>> GetUserID(string email);
}
