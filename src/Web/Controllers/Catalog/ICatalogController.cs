using Microsoft.AspNetCore.Mvc;
using VibraHeka.Domain.Entities;
using VibraHeka.Web.Entities;

namespace VibraHeka.Web.Controllers;

/// <summary>
/// Defines the contract for the Catalog API surface.
/// </summary>
public interface ICatalogController
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
    [ProducesResponseType(typeof(ResponseEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseEntity), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request, CancellationToken ct);
}
