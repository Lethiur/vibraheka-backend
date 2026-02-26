using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.Settings;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Application.Settings.Queries.GetTemplateForAction;

/// <summary>
/// Handles queries to retrieve a collection of templates associated with a specific action.
/// </summary>
/// <remarks>
/// This query handler is responsible for processing instances of <see cref="GetTemplatesForActionQuery"/>
/// and returning a list of <see cref="TemplateForActionEntity"/> objects that match the query criteria.
/// </remarks>
public class GetTemplatesForActionQueryHandler(
    ISettingsService SettingsService,
    ICurrentUserService CurrentUserService,
    ILogger<GetTemplatesForActionQueryHandler> Logger)
    : IRequestHandler<GetTemplatesForActionQuery, Result<IEnumerable<TemplateForActionEntity>>>
{
    public Task<Result<IEnumerable<TemplateForActionEntity>>> Handle(GetTemplatesForActionQuery request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(Maybe.From(CurrentUserService.UserId)
            .Where(userID =>
                !string.IsNullOrEmpty(userID) && !string.IsNullOrWhiteSpace(userID))
            .ToResult(UserErrors.InvalidUserID)
            .BindTry(_ => SettingsService.GetAllTemplatesForActions(), exception =>
            {
                Logger.LogError(exception, "Problem retrieving templates for actions");
                return AppErrors.GenericError;
            }));

    }
}
