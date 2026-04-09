using CSharpFunctionalExtensions;
using VibraHeka.Domain.User.Ports.output;

namespace VibraHeka.Application.Users.Commands.VerificationCode;

public class VerifyUserCommandHandler(UserPort userService) : IRequestHandler<VerifyUserCommand, Result<Unit>>
{
    public Task<Result<Unit>> Handle(VerifyUserCommand request, CancellationToken cancellationToken)
    {
        return userService.ConfirmUserAsync(request.Email, request.Code);
    }
}
