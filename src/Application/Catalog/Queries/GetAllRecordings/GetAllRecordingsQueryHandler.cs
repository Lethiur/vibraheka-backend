using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using VibraHeka.Application.Catalog.Models;
using VibraHeka.Application.Catalog.Queries.GetAllRecordings;
using VibraHeka.Domain.Recordings.Ports.Out;

namespace VibraHeka.Application.Recordings.Queries.GetAllRecordings;

public class GetAllRecordingsQueryHandler(
    IRecordingRegistryPort RegistryPort,
    ILogger<GetAllRecordingsQueryHandler> Logger)
    : IRequestHandler<GetAllRecordingsQuery, Result<IEnumerable<RecordingDto>>>
{
    public async Task<Result<IEnumerable<RecordingDto>>> Handle(
        GetAllRecordingsQuery request,
        CancellationToken cancellationToken)
    {
        Logger.LogInformation("Retrieving all recordings");

        return await RegistryPort
            .GetAllAsync(cancellationToken)
            .Map(recordings => recordings.Where(entity => entity.IsActive).Select(RecordingDto.FromDomain))
            .Tap(_ => Logger.LogInformation("Successfully retrieved all recordings"))
            .TapError(error => Logger.LogWarning("Failed to retrieve recordings: {Error}", error));
    }
}

