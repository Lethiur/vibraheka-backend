using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using VibraHeka.Application.Users.Commands;

namespace VibraHeka.Web.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController(IMediator mediator)
{
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
            return new OkObjectResult(new { UserId = id.Value });
        }
        return new BadRequestObjectResult(id.Error);
    }
    
    [HttpGet("test")]
    public IActionResult Test()
    {
        return new OkObjectResult(new { UserId = "test" });
    }
}
