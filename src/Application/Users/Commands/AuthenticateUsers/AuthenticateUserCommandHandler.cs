using CSharpFunctionalExtensions;
using VibraHeka.Domain.Models.Results;
using VibraHeka.Domain.User.Ports.Output;

namespace VibraHeka.Application.Users.Commands.AuthenticateUsers;

/// <summary>
/// Handles the <see cref="AuthenticateUserCommand"/> to authenticate a user.
/// Uses the <see cref="IUserService"/> to perform the authentication operation
/// and returns the result of the authentication process.
/// </summary>
public class AuthenticateUserCommandHandler(UserPort userService, UserProfilePort UserRpository) : IRequestHandler<AuthenticateUserCommand, Result<AuthenticationResult>>
{

    public Task<Result<AuthenticationResult>> Handle(AuthenticateUserCommand request, CancellationToken cancellationToken)
    {

        return userService.AuthenticateUserAsync(request.Email, request.Password)
            .Bind(authResult => UserRpository.GetProfileByUserId(authResult.UserID, cancellationToken).Map(profile =>
            {
                authResult.Role = profile.Role;
                return authResult;
            }));
    }
}
