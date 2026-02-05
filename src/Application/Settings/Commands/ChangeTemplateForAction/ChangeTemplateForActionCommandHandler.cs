using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.EmailTemplates;
using VibraHeka.Domain.Common.Interfaces.Settings;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Application.Settings.Commands.ChangeTemplateForAction;

public class ChangeTemplateForActionCommandHandler(
    ISettingsService SettingsService,
    ICurrentUserService CurrentUserService,
    IPrivilegeService PrivilegeService,
    IEmailTemplatesService EmailTemplatesService) : IRequestHandler<ChangeTemplateForActionCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(ChangeTemplateForActionCommand request, CancellationToken cancellationToken)
    {
        return await Maybe.From<string>(CurrentUserService.UserId)
            .Where(userID =>
                !string.IsNullOrEmpty(userID) && !string.IsNullOrWhiteSpace(userID))
            .ToResult(UserErrors.InvalidUserID)
            .Bind(async userID => await PrivilegeService.HasRoleAsync(userID, UserRole.Admin, cancellationToken))
            .Ensure(hasRole => hasRole, UserErrors.NotAuthorized) // Ensuring the user has admin roles
            .Bind(hasRole => EmailTemplatesService.GetTemplateByID(request.TemplateID))
            .Bind(async template =>
            {
                switch (request.ActionType)
                {
                    case ActionType.UserVerification:
                        return await SettingsService.ChangeEmailForVerificationAsync(request.TemplateID, cancellationToken);
                    default:
                        return Result.Failure<Unit>(EmailTemplateErrors.InvalidAction);
                }
            });
    }
}
