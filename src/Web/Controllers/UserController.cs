using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Users.Commands.UpdateUserProfile;
using VibraHeka.Application.Users.Queries.GetProfile;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Domain.Models.Results.User;

namespace VibraHeka.Web.Controllers;

[ApiController]
[Route("api/v1/users")]
public class UserController(IMediator mediator, ILogger<UserController> Logger)
{
    /// <summary>
    /// Retrieves the user profile associated with the specified user ID.
    /// </summary>
    /// <param name="userID">The unique identifier of the user whose profile is to be retrieved.</param>
    /// <returns>A user profile object containing details about the specified user, or null if no user is found with the given ID.</returns>
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(UserEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserProfileFromId([FromRoute(Name = "id")] string userID)
    {
        Logger.Log(LogLevel.Information, "Getting user profile for user with ID {UserID}", userID);
        GetUserProfileQuery query = new(userID);
        Result<UserDTO> result = await mediator.Send(query);
        
        if (result.IsFailure)
        {
            Logger.LogError("Failed to execute Change Template For Action because {Error}", result.Error);
            return result.Error switch
            {
                ProfileErrors.InvalidProfileID => new NotFoundObjectResult(ResponseEntity.FromError(UserErrors.UserNotFound)),
                _ => new BadRequestObjectResult(ResponseEntity.FromError(result.Error))
            };
        }
        
        return new OkObjectResult(ResponseEntity.FromSuccess(result.Value));
    }

    /// <summary>
    /// Updates the user profile with the provided new data.
    /// </summary>
    /// <param name="profile">An instance of <c>UserDTO</c> containing the updated user profile information.</param>
    /// <returns>An <c>IActionResult</c> indicating the result of the update operation. Returns a success response with the updated profile or an error response in case of failure.</returns>
    [HttpPatch("update-profile")]
    [Authorize]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<IActionResult> UpdateUserProfile([FromBody] UserDTO profile)
    {
        UpdateUserProfileCommand command = new(profile);
        Result<Unit> result = await mediator.Send(command);
        
        if (result.IsFailure)
        {
            return new BadRequestObjectResult(ResponseEntity.FromError(result.Error));
        }
        
        return new OkObjectResult(ResponseEntity.FromSuccess(result.Value));
    }
}
