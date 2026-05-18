using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibraHeka.Application.Catalog.Commands.CreateProduct;
using VibraHeka.Domain.Entities;
using VibraHeka.Web.Entities;

namespace VibraHeka.Web.Controllers;

[ApiController]
[Route("api/v1/catalog")]
public class CatalogController(IMediator mediator) : ICatalogController
{
    /// <summary>
    /// Creates a new product in the catalog.
    /// Returns the generated product ID wrapped in a <see cref="ResponseEntity"/> on success.
    /// </summary>
    /// <param name="request">Product creation payload containing name, description, price and currency.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// <c>200 OK</c> with a <see cref="ResponseEntity"/> whose <c>Content</c> is the new product ID string.<br/>
    /// <c>400 Bad Request</c> with a <see cref="ResponseEntity"/> containing the domain error code on failure.<br/>
    /// <c>401 Unauthorized</c> when the caller is not authenticated.
    /// </returns>
    [HttpPost]
    [Authorize]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ResponseEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseEntity), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request, CancellationToken ct)
    {
        CreateProductCommand command = new(
            Name: request.Name,
            Description: request.Description,
            Price: request.Price,
            CurrencyCode: request.CurrencyCode);

        Result<string> result = await mediator.Send(command, ct);

        if (result.IsFailure)
        {
            return new BadRequestObjectResult(ResponseEntity.FromError(result.Error));
        }

        return new OkObjectResult(ResponseEntity.FromSuccess(result.Value));
    }
}
