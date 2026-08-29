using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibraHeka.Application.Admin.Queries.GetAllTherapists;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Users.Commands.AdminCreateTherapist;
using VibraHeka.Application.Users.Commands.UpdateUserProfile;
using VibraHeka.Application.Users.Queries.GetProfile;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Web.Mappers;
using VibraHeka.Web.Users;

namespace VibraHeka.Web.Controllers.Users;


public class UserController(IMediator mediator, ILogger<UserController> Logger, UserMapper Mapper) : IUserController
{
    public override async Task<ActionResult<UserDTO>> GetUserDetails(string id)
    {
        Logger.Log(LogLevel.Information, "Getting user profile for user with ID {UserID}", id);
        GetUserProfileQuery query = new(id);
        Result<UserEntity> result = await mediator.Send(query);

        if (result.IsFailure)
        {
            Logger.LogError("Failed to execute Change Template For Action because {Error}", result.Error);
            return result.Error switch
            {
                ProfileErrors.InvalidProfileID => new NotFoundResult(),
                _ => new BadRequestObjectResult(result.Error)
            };
        }

        return new OkObjectResult(ResponseEntity.FromSuccess(Mapper.ToUserDto(result.Value)));
    }

    public override async Task<IActionResult> UpdateUserProfile(UpdateProfileRequest body)
    {
        UpdateUserProfileCommand command = Mapper.ToUpdateProfileCommand(body);
        Result<Unit> result = await mediator.Send(command);

        if (result.IsFailure)
        {
            return new BadRequestObjectResult(result.Error);
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
            switch (result.Error)
            {
                case UserErrors.NotAuthorized:
                    return new UnauthorizedResult();
                default:
                    return new BadRequestObjectResult(result.Error);
            }
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
            return result.Error switch
            {
                UserErrors.NotAuthorized => new UnauthorizedResult(),
                _ => new BadRequestObjectResult(result.Error)
            };
        }
        return new OkObjectResult(result.Value.Select(Mapper.ToUserDto).ToList());
    }
}
