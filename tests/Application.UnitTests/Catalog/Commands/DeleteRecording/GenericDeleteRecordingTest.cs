using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Catalog.Commands.DeleteRecording;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Ports.Out;

namespace VibraHeka.Application.UnitTests.Catalog.Commands.DeleteRecording;

public abstract class GenericDeleteRecordingTest
{
    protected Mock<IRecordingRegistryPort> RegistryPortMock = default!;
    protected Mock<IRecordingStoragePort> StoragePortMock = default!;
    protected Mock<ILogger<DeleteRecordingCommandHandler>> LoggerMock = default!;
    protected DeleteRecordingCommandHandler Handler = default!;

    [SetUp]
    public virtual void SetUp()
    {
        RegistryPortMock = new Mock<IRecordingRegistryPort>();
        StoragePortMock = new Mock<IRecordingStoragePort>();
        LoggerMock = new Mock<ILogger<DeleteRecordingCommandHandler>>();

        Handler = new DeleteRecordingCommandHandler(
            RegistryPortMock.Object,
            StoragePortMock.Object,
            LoggerMock.Object);
    }

    protected static DeleteRecordingCommand BuildValidCommand() =>
        new(RecordingId: "a1b2c3d4-e5f6-7890-abcd-ef1234567890");

    protected static RecordingEntity BuildValidEntity(string? id = null, string? storageKey = null) =>
        new()
        {
            ID = id ?? "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
            Name = "Sesion de meditacion",
            Description = "Descripcion de la sesion de meditacion guiada"
        };
}

