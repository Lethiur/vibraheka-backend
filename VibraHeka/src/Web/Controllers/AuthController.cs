using Microsoft.AspNetCore.Mvc;
using VibraHeka.Application.Users.Commands;

namespace VibraHeka.Web.Controllers;

[ApiController]
[Route("api/v1/Auth")]
public class AuthController(IMediator mediator)
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserCommand command)
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
