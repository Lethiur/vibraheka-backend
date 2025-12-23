using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Models.Results;

namespace VibraHeka.Application.Users.Commands.AuthenticateUsers;

public class AuthenticateUserCommandHandler : IRequestHandler<AuthenticateUserCommand, Result<AuthenticationResult>>
{
    public Task<Result<AuthenticationResult>> Handle(AuthenticateUserCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
