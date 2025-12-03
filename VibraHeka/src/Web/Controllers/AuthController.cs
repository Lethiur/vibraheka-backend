using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using VibraHeka.Application.Users.Commands;
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
        Result<string> id = await mediator.Send(command);

        if (!id.IsFailure)
        {
            return new OkObjectResult(ResponseEntity.FromSuccess(id.Value));
        }
        return new BadRequestObjectResult(ResponseEntity.FromError(id.Error));
    }
}
