using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Users.Commands.RefreshToken;
using VibraHeka.Application.Users.Commands.ResendConfirmationCode;
using VibraHeka.Application.Users.Commands.VerificationCode;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Models.Results;
using VibraHeka.Web.Authentication;

namespace VibraHeka.Web.Controllers.Auth;

[ApiController]
[Route("api/v1/auth")]
public class AuthController(IMediator mediator, ILogger<AuthController> Logger, AuthMapper authMapper) : IAuthController
{
    /// <summary>
    /// Handles a user registration request by processing the provided registration details
    /// and returning a result indicating success or failure.
    /// </summary>
    /// <param name="body">The command object containing the user's email, password, and full name.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the registration process.
    /// Success response contains the user ID, while failure response contains error details.</returns>
    public override async Task<ActionResult<RegisterUserResponse>> RegisterUser(RegisterUserRequest body)
    {
        Logger.LogInformation("Register endpoint called for email {Email}", body.Email);
        Result<UserRegistrationResult> id = await mediator.Send(authMapper.ToCommand(body));

        if (id.IsFailure)
        {
            Logger.LogWarning("Register endpoint failed for email {Email} with error {Error}", body.Email, id.Error);
            return new BadRequestObjectResult(new BadRequestResponse { ErrorCode = id.Error });
        }

        Logger.LogInformation("Register endpoint succeeded for email {Email} with userId {UserId}",
            body.Email, id.Value.UserId);
        return new OkObjectResult(authMapper.ToResponse(id.Value));
    }

    /// <summary>
    /// Confirms a user's account by processing the provided verification code and email address.
    /// </summary>
    /// <param name="body">The <see cref="VerifyUserCommand"/> containing the user's email and verification code.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the confirmation process.
    /// A successful response contains a success message, while a failure response includes error details.</returns>
    public override async Task<IActionResult> VerifyUser(VerifyUserRequest body)
    {
        Result<Unit> verificationResult = await mediator.Send(authMapper.ToCommand(body));

        if (verificationResult.IsFailure)
        {
            return new BadRequestObjectResult(new BadRequestResponse { ErrorCode = verificationResult.Error });
        }

        return new NoContentResult();
    }

    /// <summary>
    /// Authenticates a user by processing the provided credentials and returning a result
    /// indicating success or failure of the authentication process.
    /// </summary>
    /// <param name="body">The request object containing the user's email and password.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the authentication attempt.
    /// A successful response contains authentication details including user ID, access token, and
    /// refresh token. An error response contains the relevant error details such as invalid credentials
    /// or user not found.</returns>
    public override async Task<ActionResult<AuthenticateUserResponse>> AuthenticateUser(AuthenticateUserRequest body)
    {
        Result<AuthenticationResult> result = await mediator.Send(authMapper.ToCommand(body));

        if (result.IsFailure)
        {
            return new BadRequestObjectResult(new BadRequestResponse { ErrorCode = result.Error });
        }

        return new OkObjectResult(authMapper.ToResponse(result.Value));
    }


    /// <summary>
    /// Resends a confirmation code to the specified email address, allowing the user to verify their account.
    /// </summary>
    /// <param name="body">The request object containing the email address of the user to which the confirmation code should be resent.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.
    /// A success response confirms the code was resent, while a failure response provides error details.</returns>
    public override async Task<IActionResult> ResendConfirmationCode(ResendConfirmationCodeRequest body)
    {
        ResendConfirmationCodeCommand command = authMapper.ToCommand(body);
        Logger.LogInformation("Resending confirmation code for user with email {Email}", body.Email);
        Result<Unit> result = await mediator.Send(command);
        if (result.IsFailure)
        {
            Logger.LogError("Failed to resend confirmation code for user with email {Email}: {Error}", body.Email,
                result.Error);
            return new BadRequestObjectResult(new BadRequestResponse { ErrorCode = result.Error });
        }

        return new NoContentResult();
    }


    /// <summary>
    /// Starts the password recovery flow for the provided user email.
    /// </summary>
    /// <param name="body">Command containing the user email.</param>
    /// <returns>An <see cref="IActionResult"/> describing whether the request was accepted.</returns>
    public override async Task<IActionResult> ResetPassword(ResetPasswordRequest body)
    {
        Logger.LogInformation("Starting password recovery endpoint for email {Email}", body.Email);
        Result<Unit> result = await mediator.Send(authMapper.ToCommand(body));

        if (result.IsFailure)
        {
            Logger.LogWarning("Password recovery start failed for email {Email} with error {Error}", body.Email,
                result.Error);
            return new BadRequestObjectResult(new BadRequestResponse { ErrorCode = result.Error });
        }

        return new NoContentResult();
    }

    /// <summary>
    /// Processes a token refresh request by validating the provided refresh token and username
    /// and returns a new access token if the request is valid.
    /// </summary>
    /// <param name="body">The request object containing the refresh token and username for authentication.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the token refresh process.
    /// Success response contains the new access token, while failure response contains error details.</returns>
    public override async Task<ActionResult<RefreshTokenResponse>> RefreshToken(RefreshTokenRequest body)
    {
        RefreshTokenCommand command = authMapper.ToCommand(body);
        Result<string> result = await mediator.Send(command);
        if (result.IsFailure)
        {
            Logger.LogWarning("Refresh token failed with error {Error}", result.Error);
            return new BadRequestObjectResult(new BadRequestResponse { ErrorCode = result.Error });
        }

        return new OkObjectResult(new RefreshTokenResponse { AccessToken = result.Value });
    }


    /// <summary>
    /// Confirms password recovery using an encrypted reset token and the new password pair.
    /// </summary>
    /// <param name="body">Request an object containing an encrypted token and new password values.</param>
    /// <returns>An <see cref="IActionResult"/> with operation status.</returns>
    public override async Task<IActionResult> ConfirmResetPassword(ConfirmResetPasswordRequest body)
    {
        Logger.LogInformation("Confirming password recovery endpoint called");
        Result<Unit> result = await mediator.Send(authMapper.ToCommand(body));

        if (result.IsFailure)
        {
            Logger.LogWarning("Password recovery confirmation failed with error {Error}", result.Error);

            return result.Error switch
            {
                UserErrors.UserNotFound => new NotFoundObjectResult(ResponseEntity.FromError(result.Error)),
                _ => new BadRequestObjectResult(new BadRequestResponse { ErrorCode = result.Error })
            };
        }

        return new NoContentResult();
    }

    /// <summary>
    /// Changes password for the currently authenticated user.
    /// </summary>
    /// <param name="body">Request object containing current and new password values.</param>
    /// <returns>An <see cref="IActionResult"/> with operation status.</returns>
    public override async Task<IActionResult> ChangePassword(ChangePasswordRequest body)
    {
        Logger.LogInformation("Authenticated password change endpoint called");
        Result<Unit> result = await mediator.Send(authMapper.ToCommand(body));

        if (result.IsFailure)
        {
            Logger.LogWarning("Authenticated password change failed with error {Error}", result.Error);

            return result.Error switch
            {
                UserErrors.NotAuthorized => new UnauthorizedObjectResult(ResponseEntity.FromError(result.Error)),
                _ => new BadRequestObjectResult(new BadRequestResponse { ErrorCode = result.Error })
            };
        }

        return new NoContentResult();
    }
}
