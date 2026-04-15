using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Models.Results;

namespace VibraHeka.Application.Users.Commands.VerificationCode;

public class VerifyUserCommandHandler(IUserService userService) : IRequestHandler<VerifyUserCommand, Result<AuthenticationResult>>
{
    public Task<Result<AuthenticationResult>> Handle(VerifyUserCommand request, CancellationToken cancellationToken)
    {
        return userService.ConfirmUserAsync(request.Email, request.Code)
            .BindTry(_ => userService.AdminAuthUserAsync(request.Email, cancellationToken));
    }
}
