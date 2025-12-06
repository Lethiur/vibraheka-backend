using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Interfaces;

namespace VibraHeka.Application.Users.Commands.VerificationCode;

public class VerifyUserCommandHandler(ICognitoService cognitoService) : IRequestHandler<VerifyUserCommand, Result<Unit>>
{
    public Task<Result<Unit>> Handle(VerifyUserCommand request, CancellationToken cancellationToken)
    {
        return cognitoService.ConfirmUserAsync(request.Email, request.Code);
    }
}
