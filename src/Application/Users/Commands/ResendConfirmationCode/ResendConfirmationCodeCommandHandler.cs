using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.User;

namespace VibraHeka.Application.Users.Commands.ResendConfirmationCode;

public class ResendConfirmationCodeCommandHandler(IUserService userService, IPrivilegeService privilegeService) : IRequestHandler<ResendConfirmationCodeCommand, Result<Unit>>
{
    public Task<Result<Unit>> Handle(ResendConfirmationCodeCommand request, CancellationToken cancellationToken)
    {
        return
            userService.GetUserID(request.Email).BindTry(userID => privilegeService
                    .CanExecuteAction(userID, ActionType.RequestVerificationCode, cancellationToken))
            .Ensure(can => can, UserErrors.NotAuthorized)
            .BindTry(_ => userService.ResendVerificationCodeAsync(request.Email));


    }
}
