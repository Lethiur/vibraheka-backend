using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Users.Commands.AuthenticateUsers;
using VibraHeka.Application.Users.Commands.ConfirmPasswordRecovery;
using VibraHeka.Application.Users.Commands.RegisterUser;
using VibraHeka.Application.Users.Commands.ResendConfirmationCode;
using VibraHeka.Application.Users.Commands.StartPasswordRecovery;
using VibraHeka.Application.Users.Commands.VerificationCode;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Models.Results;

namespace VibraHeka.Web.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController(IMediator mediator, ILogger<AuthController> Logger)
{
    /// <summary>
    /// Handles a user registration request by processing the provided registration details
    /// and returning a result indicating success or failure.
    /// </summary>
    /// <param name="command">The command object containing the user's email, password, and full name.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the registration process.
    /// Success response contains the user ID, while failure response contains error details.</returns>
    [HttpPost("register")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] [Required] RegisterUserCommand command)
    {
        Logger.LogInformation("Register endpoint called for email {Email}", command.Email);
        Result<UserRegistrationResult> id = await mediator.Send(command);

        if (!id.IsFailure)
        {
            Logger.LogInformation("Register endpoint succeeded for email {Email} with userId {UserId}",
                command.Email, id.Value.UserId);
            return new OkObjectResult(ResponseEntity.FromSuccess(id.Value));
        }

        Logger.LogWarning("Register endpoint failed for email {Email} with error {Error}", command.Email, id.Error);
        return new BadRequestObjectResult(ResponseEntity.FromError(id.Error));
    }

    /// <summary>
    /// Confirms a user's account by processing the provided verification code and email address.
    /// </summary>
    /// <param name="command">The <see cref="VerifyUserCommand"/> containing the user's email and verification code.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the confirmation process.
    /// A successful response contains a success message, while a failure response includes error details.</returns>
    [HttpPatch("confirm")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmUser([FromBody] [Required] VerifyUserCommand command)
    {
        Result<Unit> verificationResult = await mediator.Send(command);


        if (verificationResult.IsFailure)
        {
            switch (verificationResult.Error)
            {
                case UserErrors.UserNotFound:
                    return new NotFoundObjectResult(ResponseEntity.FromError(verificationResult.Error));
                case UserErrors.InvalidVerificationCode:
                case UserErrors.WrongVerificationCode:
                    return new BadRequestObjectResult(ResponseEntity.FromError(verificationResult.Error));
            }

            return new BadRequestObjectResult(ResponseEntity.FromError(verificationResult.Error));
        }

        return new OkObjectResult(ResponseEntity.FromSuccess(verificationResult.Value));
    }

    /// <summary>
    /// Authenticates a user by processing the provided credentials and returning a result
    /// indicating success or failure of the authentication process.
    /// </summary>
    /// <param name="command">The command object containing the user's email and password.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the authentication attempt.
    /// A successful response contains authentication details including user ID, access token, and
    /// refresh token. An error response contains the relevant error details such as invalid credentials
    /// or user not found.</returns>
    [HttpPost("authenticate")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Authenticate([FromBody] [Required] AuthenticateUserCommand command)
    {
        Result<AuthenticationResult> result = await mediator.Send(command);

        if (result.IsFailure)
        {
            switch (result.Error)
            {
                case UserErrors.NotAuthorized:
                    return new NotFoundObjectResult(ResponseEntity.FromError(UserErrors.InvalidPassword));
                case UserErrors.UserNotFound:
                case UserErrors.InvalidPassword:
                    return new NotFoundObjectResult(ResponseEntity.FromError(result.Error));
                case UserErrors.UserNotConfirmed:
                    return new BadRequestObjectResult(ResponseEntity.FromError(result.Error));
            }
        }

        return new OkObjectResult(ResponseEntity.FromSuccess(result.Value));
    }

    /// <summary>
    /// Resends a confirmation code to the specified email address, allowing the user to verify their account.
    /// </summary>
    /// <param name="email">The email address of the user to which the confirmation code should be resent.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.
    /// A success response confirms the code was resent, while a failure response provides error details.</returns>
    [HttpGet("resend-confirmation-code")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResendConfirmationCode([FromQuery(Name = "email")] string email)
    {
        ResendConfirmationCodeCommand command = new(email);
        Logger.LogInformation("Resending confirmation code for user with email {Email}", email);
        Result<Unit> result = await mediator.Send(command);
        if (result.IsFailure)
        {
            Logger.LogError("Failed to resend confirmation code for user with email {Email}: {Error}", email, result.Error);
            return new BadRequestObjectResult(ResponseEntity.FromError(result.Error));
        }

        return new OkObjectResult(ResponseEntity.FromSuccess("Confirmation code resent successfully"));
    }

    /// <summary>
    /// Starts the password recovery flow for the provided user email.
    /// </summary>
    /// <param name="command">Command containing the user email.</param>
    /// <returns>An <see cref="IActionResult"/> describing whether the request was accepted.</returns>
    [HttpPost("forgot-password")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StartPasswordRecovery([FromBody] [Required] StartPasswordRecoveryCommand command)
    {
        Logger.LogInformation("Starting password recovery endpoint for email {Email}", command.Email);
        Result<Unit> result = await mediator.Send(command);

        if (result.IsFailure)
        {
            Logger.LogWarning("Password recovery start failed for email {Email} with error {Error}", command.Email, result.Error);
            return new BadRequestObjectResult(ResponseEntity.FromError(result.Error));
        }

        return new OkObjectResult(ResponseEntity.FromSuccess("If the account exists, a recovery email has been sent."));
    }

    /// <summary>
    /// Confirms password recovery using an encrypted reset token and the new password pair.
    /// </summary>
    /// <param name="command">Command containing encrypted token and new password values.</param>
    /// <returns>An <see cref="IActionResult"/> with operation status.</returns>
    [HttpPost("forgot-password/confirm")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmPasswordRecovery([FromBody] [Required] ConfirmPasswordRecoveryCommand command)
    {
        Logger.LogInformation("Confirming password recovery endpoint called");
        Result<Unit> result = await mediator.Send(command);

        if (result.IsFailure)
        {
            Logger.LogWarning("Password recovery confirmation failed with error {Error}", result.Error);

            return result.Error switch
            {
                UserErrors.UserNotFound => new NotFoundObjectResult(ResponseEntity.FromError(result.Error)),
                UserErrors.InvalidPasswordResetToken => new BadRequestObjectResult(ResponseEntity.FromError(result.Error)),
                UserErrors.PasswordResetTokenExpired => new BadRequestObjectResult(ResponseEntity.FromError(result.Error)),
                UserErrors.PasswordResetTokenAlreadyUsed => new BadRequestObjectResult(ResponseEntity.FromError(result.Error)),
                UserErrors.InvalidPassword => new BadRequestObjectResult(ResponseEntity.FromError(result.Error)),
                UserErrors.WrongVerificationCode => new BadRequestObjectResult(ResponseEntity.FromError(result.Error)),
                UserErrors.ExpiredCode => new BadRequestObjectResult(ResponseEntity.FromError(result.Error)),
                _ => new BadRequestObjectResult(ResponseEntity.FromError(result.Error))
            };
        }

        return new OkObjectResult(ResponseEntity.FromSuccess(Unit.Value));
    }
}
