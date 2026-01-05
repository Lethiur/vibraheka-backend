using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Common.Interfaces;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Application.Users.Commands.AdminCreateTherapist;

/// <summary>
/// Handles the command to create a new therapist user in the system.
/// </summary>
/// <remarks>
/// This class processes the <see cref="CreateTherapistCommand"/> by verifying the current user's
/// administrative privileges, generating a default password for the new user, and delegating
/// creation responsibilities to the underlying user repository and authentication service.
/// </remarks>
public class CreateTherapistCommandHandler(
    ICognitoService CognitService,
    IUserRepository Repository,
    ICurrentUserService CurrentUserService,
    IPrivilegeService PrivilegeService)
    : IRequestHandler<CreateTherapistCommand, Result<string>>
{
    /// <summary>
    /// Handles the creation of a new therapist user in the system.
    /// </summary>
    /// <param name="request">The command containing the therapist's email and name.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>A Result object containing the created user's ID if successful, or an error message if the operation fails.</returns>
    public async Task<Result<string>> Handle(CreateTherapistCommand request, CancellationToken cancellationToken)
    {
        string password = "Password123!@#";

        Result<bool> checkPrivilegesResult = await PrivilegeService.HasRoleAsync(CurrentUserService.UserId ?? "", UserRole.Admin);

        if (checkPrivilegesResult is { IsSuccess: true, Value: true })
        {
            Result<string> cognitoIdResult = await CognitService.RegisterUserAsync(request.Email, password, request.Name);

            return await cognitoIdResult.Bind(async id =>
            {
                User user = new()
                {
                    FullName = request.Name,
                    Email = request.Email,
                    Id = id,
                    CognitoId = id,
                    Role = UserRole.Therapist
                };

                return await Repository.AddAsync(user);
            });    
        }

        return Result.Failure<string>(UserException.NotAuthorized);
    }
}
