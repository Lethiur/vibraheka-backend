using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Models.Results;

namespace VibraHeka.Domain.User.Ports.output;

public interface UserPort
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
    /// Starts the password recovery flow in Cognito for the specified email.
    /// </summary>
    /// <param name="email">The email address associated with the account.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A result indicating whether the recovery process was started.</returns>
    Task<Result<Unit>> StartPasswordRecoveryAsync(string email, CancellationToken cancellationToken);

    /// <summary>
    /// Confirms password recovery in Cognito using the recovery code and the new password.
    /// </summary>
    /// <param name="email">Email associated with the account.</param>
    /// <param name="recoveryCode">Recovery code issued by Cognito.</param>
    /// <param name="newPassword">New password to set.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A result indicating whether the password was successfully updated.</returns>
    Task<Result<Unit>> ConfirmPasswordRecoveryAsync(string email, string recoveryCode, string newPassword, CancellationToken cancellationToken);

    /// <summary>
    /// Changes the current user's password in Cognito using a valid access token.
    /// </summary>
    /// <param name="accessToken">Access token from the authenticated session.</param>
    /// <param name="currentPassword">Current user password.</param>
    /// <param name="newPassword">New password to set.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result<Unit>> ChangePasswordAsync(
        string accessToken,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously retrieves the unique identifier of a user from AWS Cognito based on the provided email address.
    /// </summary>
    /// <param name="email">The email address of the user whose unique identifier is being retrieved.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a <see cref="Result"/> where T is a string representing the unique identifier of the user, if found.
    /// </returns>
    Task<Result<string>> GetUserID(string email, CancellationToken cancellationToken);
    
    /// <summary>
    /// Asynchronously refreshes an authentication token for the user using the provided refresh token and email.
    /// </summary>
    /// <param name="refreshToken">The refresh token used to generate a new authentication token.</param>
    /// <param name="email">The email address of the user associated with the refresh token.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a <see cref="Result"/> indicating the success or failure of the token refresh operation.
    /// </returns>
    Task<Result<string>> RefreshToken(string refreshToken, string email, CancellationToken cancellationToken);
}
