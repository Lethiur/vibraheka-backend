using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using VibraHeka.Application.Users.Commands.AdminCreateTherapist;
using VibraHeka.Application.Users.Commands.UpdateUserProfile;
using VibraHeka.Application.Users.Queries.AdminGetTherapists;
using VibraHeka.Application.Users.Queries.GetProfile;
using VibraHeka.Domain.Entities;
using VibraHeka.Web.Users;

namespace VibraHeka.Web.Controllers.Users;


public class UserController(IMediator mediator, ILogger<UserController> Logger, UserMapper Mapper) : IUserController
{
    /// <summary>
    /// Retrieves the details of a user based on the provided user ID.
    /// </summary>
    /// <param name="id">The ID of the user whose details are to be retrieved.</param>
    /// <returns>An <see cref="ActionResult{UserDTO}"/> containing the user details if successful, or an error response if the operation fails.</returns>
    public override async Task<ActionResult<UserDTO>> GetUserDetails(string id)
    {
        Logger.Log(LogLevel.Information, "Getting user profile for user with ID {UserID}", id);
        GetUserProfileQuery query = new(id);
        Result<UserEntity> result = await mediator.Send(query);

        if (result.IsFailure)
        {
            Logger.LogError("Failed to execute Change Template For Action because {Error}", result.Error);
            return new BadRequestObjectResult(new BadRequestResponse { ErrorCode = result.Error });
        }

        return new OkObjectResult(ResponseEntity.FromSuccess(Mapper.ToUserDto(result.Value)));
    }

    /// <summary>
    /// Updates the user profile with the provided information.
    /// </summary>
    /// <param name="body">The request object containing the updated user profile information.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the update operation.
    /// A successful response returns a 204 No Content status, while a failure response returns a 400 Bad Request with error details.</returns>
    public override async Task<IActionResult> UpdateUserProfile(UpdateProfileRequest body)
    {
        UpdateUserProfileCommand command = Mapper.ToUpdateProfileCommand(body);
        Result<Unit> result = await mediator.Send(command);

        if (result.IsFailure)
        {
            return new BadRequestObjectResult(new BadRequestResponse { ErrorCode = result.Error });
        }

        return new NoContentResult();
    }
    
     /// <summary>
    /// Handles the creation of a new therapist by processing the provided command data.
    /// </summary>
    /// <param name="command">An instance of <c>CreateTherapistCommand</c> containing the email and name of the therapist to be created.</param>
    /// <returns>
    /// An <c>IActionResult</c> representing the HTTP response. If the operation is successful, returns a 200 OK response
    /// with the created therapist's identifier. If the operation fails, returns a 400 Bad Request with the error details
    /// or a 401 Unauthorized if the user lacks appropriate authorization.
    /// </returns>
    public override async Task<ActionResult<string>> CreateTherapist(CreateTherapistRequest command)
    {
        CreateTherapistCommand createTherapistCommand = Mapper.ToCreateTherapistCommand(command);
        Result<string> result = await mediator.Send(createTherapistCommand);

        if (result.IsFailure)
        {
            return new BadRequestObjectResult(new BadRequestResponse { ErrorCode = result.Error });

        }
        return new OkObjectResult(result.Value);
    }

    /// <summary>
    /// Retrieves a list of all therapists accessible to the administrator.
    /// </summary>
    /// <returns>
    /// An <c>IActionResult</c> representing the HTTP response. Returns a 200 OK response with a list
    /// of therapists if the operation is successful. If the operation fails, returns a 400 Bad Request
    /// response with the error details, or a 401 Unauthorized response if the user lacks sufficient authorization.
    /// </returns>
    public override async Task<ActionResult<ICollection<UserDTO>>> GetAllTherapists()
    {
        Result<List<UserEntity>> result = await mediator.Send(new GetAllTherapistsQuery());

        if (result.IsFailure)
        {
            return new BadRequestObjectResult(new BadRequestResponse { ErrorCode = result.Error });

        }
        return new OkObjectResult(result.Value.Select(Mapper.ToUserDto).ToList());
    }
}
