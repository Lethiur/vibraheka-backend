using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.User;
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
    IUserService CognitService,
    IUserRepository Repository,
    ICurrentUserService CurrentUserService)
    : IRequestHandler<CreateTherapistCommand, Result<string>>
{
    /// <summary>
    /// Handles the creation of a new therapist user in the system.
    /// </summary>
    /// <param name="request">The command containing the therapist's email and name.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>A Result object containing the created user's ID if successful, or an error message if the operation fails.</returns>
    public Task<Result<string>> Handle(CreateTherapistCommand request, CancellationToken cancellationToken)
    {
        const string password = "Password123!@#";
        return CognitService.RegisterUserAsync(request.Email, password, request.Name).Bind(async id =>
        {
            User user = new()
            {
                FullName = request.Name,
                Email = request.Email,
                Id = id,
                CognitoId = id,
                Role = UserRole.Therapist,
                Created = DateTime.UtcNow,
                CreatedBy = CurrentUserService.UserId,
                LastModified = DateTime.UtcNow,
                LastModifiedBy = CurrentUserService.UserId
            };

            return await Repository.AddAsync(user);
        });
    }
}
