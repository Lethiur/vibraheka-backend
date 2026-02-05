using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Models.Results;

namespace VibraHeka.Application.Users.Commands.AuthenticateUsers;

/// <summary>
/// Handles the <see cref="AuthenticateUserCommand"/> to authenticate a user.
/// Uses the <see cref="IUserService"/> to perform the authentication operation
/// and returns the result of the authentication process.
/// </summary>
public class AuthenticateUserCommandHandler(IUserService userService, IUserRepository UserRpository) : IRequestHandler<AuthenticateUserCommand, Result<AuthenticationResult>>
{

    public async Task<Result<AuthenticationResult>> Handle(AuthenticateUserCommand request, CancellationToken cancellationToken)
    {
        Result<AuthenticationResult> authenticateUserAsync = await userService.AuthenticateUserAsync(request.Email, request.Password);

        return await authenticateUserAsync.Bind(async (result) =>
        {
            return (await UserRpository.GetByIdAsync(result.UserID, cancellationToken)).Map(user =>
            {
                result.Role = user.Role;
                return result;
            });
        });

    }
}
