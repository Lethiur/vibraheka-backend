using System.ComponentModel.DataAnnotations;
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
        var id = await mediator.Send(command);
        return new OkObjectResult(new { UserId = id });
    }
    
    [HttpGet("test")]
    public IActionResult Test()
    {
        return new OkObjectResult(new { UserId = "test" });
    }
}
