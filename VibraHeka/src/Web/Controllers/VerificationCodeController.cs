using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using VibraHeka.Application.Users.Queries.GetCode;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Web.Controllers;


[ApiController]
[Route("api/v1/auth")]
public class VerificationCodeController(IMediator mediator)
{
    
    [HttpPost("verification-code")]
    public async Task<IActionResult> Register([FromBody] [Required] GetCodeQuery query)
    {
        Result<VerificationCodeEntity> id = await mediator.Send(query);

        if (!id.IsFailure)
        {
            return new OkObjectResult(ResponseEntity.FromSuccess(id.Value));
        }
        return new BadRequestObjectResult(ResponseEntity.FromError(id.Error));
    }
}
