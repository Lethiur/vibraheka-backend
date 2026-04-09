using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Domain.User.Ports.output;

namespace VibraHeka.Application.Users.Commands.ResendConfirmationCode;

public class ResendConfirmationCodeCommandHandler(UserPort userService, ActionLogPort privilegeService)
    : IRequestHandler<ResendConfirmationCodeCommand, Result<Unit>>
{
    public Task<Result<Unit>> Handle(ResendConfirmationCodeCommand request, CancellationToken cancellationToken)
    {
        return
            userService.GetUserID(request.Email, cancellationToken)
                .Bind(userID =>
                    privilegeService.GetActionLogForUser(userID, ActionType.RequestVerificationCode, cancellationToken)
                        .OnFailureCompensate(error =>
                        {
                            if (error == ActionLogErrors.ActionLogNotFound)
                            {
                                return Result.Success(new ActionLogEntity()
                                {
                                    Action = ActionType.RequestVerificationCode,
                                    ID = userID,
                                    Timestamp = DateTime.UnixEpoch,
                                });
                            }

                            return Result.Failure<ActionLogEntity>(error);
                        }))
                .Ensure(can => (can.Timestamp - DateTime.UnixEpoch).Minutes > 1, UserErrors.NotAuthorized)
                .BindTry(entity => privilegeService.SaveActionLog(entity, cancellationToken))
                .BindTry(_ => userService.ResendVerificationCodeAsync(request.Email));
    }
}
