using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Common.Models.Results;
using VibraHeka.Application.Users.Commands;
using VibraHeka.Application.Users.Commands.VerificationCode;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Web.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController(IMediator mediator)
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
        Result<UserRegistrationResult> id = await mediator.Send(command);

        if (!id.IsFailure)
        {
            return new OkObjectResult(ResponseEntity.FromSuccess(id.Value));
        }
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
        
        if (!verificationResult.IsFailure)
        {
            return new OkObjectResult(ResponseEntity.FromSuccess(verificationResult.Value));
        }

        if (verificationResult.IsFailure)
        {
            switch (verificationResult.Error)
            {
                case UserException.UserNotFound:
                    return new NotFoundObjectResult(ResponseEntity.FromError(verificationResult.Error));
                case UserException.InvalidVerificationCode:
                    return new BadRequestObjectResult(ResponseEntity.FromError(verificationResult.Error));
            }
        }
        return new BadRequestObjectResult(ResponseEntity.FromError(verificationResult.Error));
    }
}
