using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Interfaces;
using VibraHeka.Application.Common.Models.Results;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Application.Users.Commands.AuthenticateUsers;

/// <summary>
/// Handles the <see cref="AuthenticateUserCommand"/> to authenticate a user.
/// Uses the <see cref="ICognitoService"/> to perform the authentication operation
/// and returns the result of the authentication process.
/// </summary>
public class AuthenticateUserCommandHandler(ICognitoService CognitoService, IUserRepository UserRpository) : IRequestHandler<AuthenticateUserCommand, Result<AuthenticationResult>>
{

    public async Task<Result<AuthenticationResult>> Handle(AuthenticateUserCommand request, CancellationToken cancellationToken)
    {
        Result<AuthenticationResult> authenticateUserAsync = await CognitoService.AuthenticateUserAsync(request.Email, request.Password);

        return await authenticateUserAsync.Bind(async (result) =>
        {
            return (await UserRpository.GetByIdAsync(result.UserID)).Map(user =>
            {
                result.Role = user.Role;
                return result;
            });
        });

    }
}
