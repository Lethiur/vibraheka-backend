using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Recordings.Commnads.AdminAddRecording;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Recordings.Entities;
using VibraHeka.Domain.Recordings.Enums;
using VibraHeka.Domain.Recordings.Ports.Out;

namespace VibraHeka.Application.UnitTests.Recordings.Commands.AdminAddRecording;

public abstract class GenericAdminAddRecordingTest
{
    protected Mock<IRecordingStoragePort> StoragePortMock = default!;
    protected Mock<IRecordingRegistryPort> RegistryPortMock = default!;
    protected Mock<ICurrentUserService> CurrentUserServiceMock = default!;
    protected Mock<ILogger<AdminAddRecordingCommandHandler>> LoggerMock = default!;
    protected AdminAddRecordingCommandHandler Handler = default!;

    [SetUp]
    public virtual void SetUp()
    {
        StoragePortMock = new Mock<IRecordingStoragePort>();
        RegistryPortMock = new Mock<IRecordingRegistryPort>();
        CurrentUserServiceMock = new Mock<ICurrentUserService>();
        LoggerMock = new Mock<ILogger<AdminAddRecordingCommandHandler>>();

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns("admin-user-id");

        Handler = new AdminAddRecordingCommandHandler(
            StoragePortMock.Object,
            RegistryPortMock.Object,
            CurrentUserServiceMock.Object,
            LoggerMock.Object);
    }

    protected static AdminAddRecordingCommand BuildValidCommand() =>
        AdminAddRecordingCommandBuilder.BuildValid();

    protected static RecordingEntity BuildValidEntity(string recordingId, string storageKey) =>
        new()
        {
            Id = recordingId,
            Name = "Sesion de meditacion",
            Description = "Descripcion de la sesion de meditacion guiada",
            Type = RecordingType.Meditacion,
            Created = DateTimeOffset.UtcNow,
            CreatedBy = "admin-user-id",
        };
}

