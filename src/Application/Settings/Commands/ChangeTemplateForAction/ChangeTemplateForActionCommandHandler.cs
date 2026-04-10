using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.EmailTemplates.Models;
using VibraHeka.Domain.EmailTemplates.Ports.Out;
using VibraHeka.Domain.User.Services;

namespace VibraHeka.Application.Settings.Commands.ChangeTemplateForAction;

public class ChangeTemplateForActionCommandHandler(
    EmailTemplateConfigPort SettingsService,
    ICurrentUserService CurrentUserService,
    EmailTemplatePort EmailTemplatesService,
    ILogger<ChangeTemplateForActionCommandHandler> logger)
    : IRequestHandler<ChangeTemplateForActionCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(ChangeTemplateForActionCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Executing command for changing template for action: {ActionType}", request.ActionType);
        return await Maybe.From(CurrentUserService.UserId)
            .Where(userID => !string.IsNullOrEmpty(userID) && !string.IsNullOrWhiteSpace(userID))
            .ToResult(UserErrors.InvalidUserID)
            .BindTry(_ => EmailTemplatesService.GetTemplateByID(request.TemplateID, cancellationToken))
            .BindTry( entity => SettingsService.ChangeEmailTemplateKeyForAction(ActionTypeModel.ActionTypes[request.ActionType],
                entity.TemplateID, cancellationToken));
    }
}
