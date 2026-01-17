using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Web.Controllers;

[ApiController]
[Route("api/v1/email-templates")]
public class EmailTemplateController
{
    [HttpGet]
    [Authorize]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<IActionResult> GetTemplates()
    {
        return new OkObjectResult(ResponseEntity.FromSuccess(""));
    }
}
