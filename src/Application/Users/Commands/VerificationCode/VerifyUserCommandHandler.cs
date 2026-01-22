using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Interfaces.User;

namespace VibraHeka.Application.Users.Commands.VerificationCode;

public class VerifyUserCommandHandler(IUserService userService) : IRequestHandler<VerifyUserCommand, Result<Unit>>
{
    public Task<Result<Unit>> Handle(VerifyUserCommand request, CancellationToken cancellationToken)
    {
        return userService.ConfirmUserAsync(request.Email, request.Code);
    }
}
