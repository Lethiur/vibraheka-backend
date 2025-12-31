using CSharpFunctionalExtensions;

namespace VibraHeka.Application.Users.Commands.VerificationCode;

/// <summary>
/// Represents a command to verify a user through email and verification code.
/// </summary>
/// <remarks>
/// This command is used to confirm a user account by validating the provided
/// email address and verification code. It is handled by <see cref="VerifyUserCommandHandler"/>.
/// The verification process typically involves communication with a service
/// such as AWS Cognito to confirm the user's account.
/// </remarks>
/// <param name="Email">The registered email address of the user being verified.</param>
/// <param name="Code">The verification code sent to the user's email.</param>
/// <returns>
/// A result encapsulated in a <see cref="Result{T}"/> object. If the verification succeeds,
/// the result will contain a <see cref="Unit"/> instance. If it fails, an error message will be provided.
/// </returns>
public record VerifyUserCommand(string Email, string Code) : IRequest<Result<Unit>>;
