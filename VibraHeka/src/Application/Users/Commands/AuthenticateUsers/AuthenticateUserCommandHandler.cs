using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Interfaces;
using VibraHeka.Application.Common.Models.Results;

namespace VibraHeka.Application.Users.Commands.AuthenticateUsers;

/// <summary>
/// Handles the <see cref="AuthenticateUserCommand"/> to authenticate a user.
/// Uses the <see cref="ICognitoService"/> to perform the authentication operation
/// and returns the result of the authentication process.
/// </summary>
public class AuthenticateUserCommandHandler(ICognitoService CognitoService) : IRequestHandler<AuthenticateUserCommand, Result<AuthenticationResult>>
{

    public async Task<Result<AuthenticationResult>> Handle(AuthenticateUserCommand request, CancellationToken cancellationToken)
    {
        return await CognitoService.AuthenticateUserAsync(request.Email, request.Password);
    }
}
