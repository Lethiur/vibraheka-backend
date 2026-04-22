using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Recordings.Queries.GetRecordingDownloadUrl;
using VibraHeka.Domain.Recordings.Entities;
using VibraHeka.Domain.Recordings.Enums;
using VibraHeka.Domain.Recordings.Ports.Out;

namespace VibraHeka.Application.UnitTests.Recordings.Queries.GetRecordingDownloadUrl;

public abstract class GenericGetRecordingDownloadUrlTest
{
    protected Mock<IRecordingRegistryPort> RegistryPortMock = default!;
    protected Mock<IRecordingStoragePort> StoragePortMock = default!;
    protected Mock<ILogger<GetRecordingDownloadUrlQueryHandler>> LoggerMock = default!;
    protected GetRecordingDownloadUrlQueryHandler Handler = default!;

    [SetUp]
    public virtual void SetUp()
    {
        RegistryPortMock = new Mock<IRecordingRegistryPort>();
        StoragePortMock = new Mock<IRecordingStoragePort>();
        LoggerMock = new Mock<ILogger<GetRecordingDownloadUrlQueryHandler>>();
        Handler = new GetRecordingDownloadUrlQueryHandler(
            RegistryPortMock.Object,
            StoragePortMock.Object,
            LoggerMock.Object);
    }

    protected static GetRecordingDownloadUrlQuery BuildValidQuery() =>
        new(Guid.NewGuid().ToString());

    protected static GetRecordingDownloadUrlQuery BuildValidQuery(string recordingId) =>
        new(recordingId);

    protected static ValidationResult ValidValidationResult() => new();

    protected static ValidationResult InvalidValidationResult(string errorMessage) =>
        new(
            new List<ValidationFailure>
            {
                new ValidationFailure(nameof(GetRecordingDownloadUrlQuery.RecordingId), errorMessage),
            });

    protected static RecordingEntity BuildRecordingEntity(string recordingId, string storageKey) =>
        new()
        {
            Id = recordingId,
            Name = "Meditacion matutina",
            Description = "Una sesion de meditacion para empezar el dia",
            Type = RecordingType.Meditacion,
            Created = DateTimeOffset.UtcNow,
            CreatedBy = "admin-user-id",
        };
}
