using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Recordings.Ports.Out;

namespace VibraHeka.Application.Catalog.Queries.AdminGetRecordings;

public class AdminGetRecordingsQueryHandler(
    IRecordingRegistryPort RegistryPort,
    ILogger<AdminGetRecordingsQueryHandler> Logger)
    : IRequestHandler<AdminGetRecordingsQuery, Result<IEnumerable<RecordingEntity>>>
{
    public async Task<Result<IEnumerable<RecordingEntity>>> Handle(
        AdminGetRecordingsQuery request,
        CancellationToken cancellationToken)
    {
        Logger.LogInformation("Retrieving all recordings");

        return await RegistryPort
            .GetAllAsync(cancellationToken)
            .Tap(_ => Logger.LogInformation("Successfully retrieved all recordings"))
            .TapError(error => Logger.LogWarning("Failed to retrieve recordings: {Error}", error));
    }
}
