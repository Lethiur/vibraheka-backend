using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using NMoneys;
using VibraHeka.Application.Recordings.Commnands.AdminAddRecording;
using VibraHeka.Application.Recordings.Entities;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Enums;
using VibraHeka.Domain.Catalog.Ports.In;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Recordings.Ports.Out;

namespace VibraHeka.Application.Catalog.Commands.AdminAddRecording;

public sealed class AdminAddRecordingCommandHandler(
    IRecordingStoragePort StoragePort,
    IRecordingRegistryPort RegistryPort,
    IRegisterSellableItemPort SellableItemPort,
    ICurrentUserService CurrentUserService,
    ILogger<AdminAddRecordingCommandHandler> Logger)
    : IRequestHandler<AdminAddRecordingCommand, Result<AddRecordingResult>>
{
    public async Task<Result<AddRecordingResult>> Handle(
        AdminAddRecordingCommand request,
        CancellationToken cancellationToken)
    {
        string recordingId = Guid.NewGuid().ToString();

        Logger.LogInformation("Creating recording entry {RecordingId} with name {Name}, tier {Tier}", recordingId,
            request.Name, request.Tier);

        RecordingEntity entity = new()
        {
            RecordingID = recordingId,
            Name = request.Name,
            Description = request.Description,
            Tier = request.Tier,
            IsActive = true,
            RecordingType = request.Type,
            Created = DateTimeOffset.UtcNow,
            CreatedBy = CurrentUserService.UserId,
            LastModified = DateTimeOffset.UtcNow,
            LastModifiedBy = CurrentUserService.UserId
        };

        Result<string> saveResult = await RegistryPort.SaveRecording(entity, cancellationToken);
        if (saveResult.IsFailure)
        {
            Logger.LogWarning("Failed to persist recording {RecordingId}: {Error}", recordingId, saveResult.Error);
            return Result.Failure<AddRecordingResult>(saveResult.Error);
        }

        Result<Unit> productRegistrationResult =
            await SellableItemPort.RegisterSellableItemAsync(entity, new Money(request.Price, request.CurrencyCode),
                PriceKind.OneTime, null, cancellationToken);

        if (productRegistrationResult.IsFailure)
        {
            Logger.LogWarning("Failed to register sellable item for recording {RecordingId}: {Error}", recordingId,
                productRegistrationResult.Error);
            return Result.Failure<AddRecordingResult>(productRegistrationResult.Error);
        }

        Logger.LogInformation(
            "Recording {RecordingId} persisted. Generating pre-signed upload URL for",
            recordingId);

        Result<string> urlResult = await StoragePort.GetUploadUrlAsync(entity.RecordingID, cancellationToken);
        if (urlResult.IsFailure)
        {
            Logger.LogWarning("Failed to generate upload URL for recording {RecordingId}: {Error}", recordingId,
                urlResult.Error);
            return Result.Failure<AddRecordingResult>(urlResult.Error);
        }

        Logger.LogInformation("Recording {RecordingId} ready for direct upload", entity.RecordingID);
        return Result.Success(new AddRecordingResult(entity.RecordingID, urlResult.Value));
    }
}
