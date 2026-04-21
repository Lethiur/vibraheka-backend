using System.ComponentModel;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Recordings.Commnads.AdminAddRecording;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Recordings.Entities;
using VibraHeka.Domain.Recordings.Enums;
using VibraHeka.Domain.Recordings.Errors;
using VibraHeka.Domain.Recordings.Ports.Out;

namespace VibraHeka.Application.UnitTests.Recordings.Commands.AdminAddRecording;

[TestFixture]
public class AdminAddRecordingCommandHandlerTest
{
    private Mock<IRecordingStoragePort> StoragePortMock = default!;
    private Mock<IRecordingRegistryPort> RegistryPortMock = default!;
    private Mock<ICurrentUserService> CurrentUserServiceMock = default!;
    private Mock<ILogger<AdminAddRecordingCommandHandler>> LoggerMock = default!;
    private AdminAddRecordingCommandHandler Handler = default!;

    [SetUp]
    public void SetUp()
    {
        StoragePortMock = new Mock<IRecordingStoragePort>();
        RegistryPortMock = new Mock<IRecordingRegistryPort>();
        CurrentUserServiceMock = new Mock<ICurrentUserService>();
        LoggerMock = new Mock<ILogger<AdminAddRecordingCommandHandler>>();

        Handler = new AdminAddRecordingCommandHandler(
            StoragePortMock.Object,
            RegistryPortMock.Object,
            CurrentUserServiceMock.Object,
            LoggerMock.Object);
    }

    #region Happy Path Tests

    [Test]
    [DisplayName("Should return success with recording ID when upload and save succeed")]
    public async Task ShouldReturnSuccessWithRecordingIdWhenUploadAndSaveSucceed()
    {
        // Given: a valid command, storage returns a storage key, and registry saves successfully
        Stream fileStream = new MemoryStream(new byte[] { 1, 2, 3 });
        AdminAddRecordingCommand command = new(
            Name: "Meditacion matutina",
            Description: "Una sesion de meditacion para empezar el dia",
            Type: RecordingType.Meditacion,
            FileStream: fileStream,
            FileName: "meditacion.mp4");

        string adminUserId = "admin-user-id";
        string storageKey = "recordings/some-id/meditacion.mp4";

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(adminUserId);

        StoragePortMock
            .Setup(x => x.UploadAsync(It.IsAny<string>(), fileStream, command.FileName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(storageKey));

        RegistryPortMock
            .Setup(x => x.SaveRecording(It.IsAny<RecordingEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RecordingEntity entity, CancellationToken _) => Result.Success(entity.Id));

        // When: the handler processes the command
        Result<string> result = await Handler.Handle(command, CancellationToken.None);

        // Then: the result should be success and contain a non-empty recording ID
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");
        Assert.That(result.Value, Is.Not.Null.And.Not.Empty,
            "Expected a non-empty recording ID but got null or empty");

        StoragePortMock.Verify(
            x => x.UploadAsync(
                It.Is<string>(id => !string.IsNullOrEmpty(id)),
                It.Is<Stream>(s => ReferenceEquals(s, fileStream)),
                It.Is<string>(fn => fn == command.FileName),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected UploadAsync to be called exactly once with the provided stream and fileName");

        RegistryPortMock.Verify(
            x => x.SaveRecording(
                It.Is<RecordingEntity>(e =>
                    e.Name == command.Name &&
                    e.Description == command.Description &&
                    e.Type == command.Type &&
                    e.StorageKey == storageKey &&
                    e.CreatedBy == adminUserId &&
                    !string.IsNullOrEmpty(e.Id)),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected SaveAsync to be called once with entity containing correct Name, Description, Type, StorageKey and CreatedBy");

        StoragePortMock.VerifyNoOtherCalls();
        RegistryPortMock.VerifyNoOtherCalls();
    }

    #endregion

    #region Railway Pattern Tests

    [Test]
    [DisplayName("Should not call RegistryPort when StoragePort fails (Railway)")]
    public async Task ShouldNotCallRegistryPortWhenStoragePortFails()
    {
        // Given: a valid command but storage upload fails
        Stream fileStream = new MemoryStream(new byte[] { 1, 2, 3 });
        AdminAddRecordingCommand command = new(
            Name: "Masterclass de yoga",
            Description: "Clase completa de yoga para principiantes",
            Type: RecordingType.Masterclass,
            FileStream: fileStream,
            FileName: "yoga.mp4");

        string uploadError = RecordingErrors.UploadFailed;

        CurrentUserServiceMock.Setup(x => x.UserId).Returns("admin-user-id");

        StoragePortMock
            .Setup(x => x.UploadAsync(It.IsAny<string>(), fileStream, command.FileName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<string>(uploadError));

        // When: the handler processes the command
        Result<string> result = await Handler.Handle(command, CancellationToken.None);

        // Then: result should be failure and RegistryPort should never be invoked
        Assert.That(result.IsSuccess, Is.False,
            $"Expected failure when StoragePort fails, but got success with value: '{(result.IsSuccess ? result.Value : "N/A")}'");
        Assert.That(result.Error, Is.EqualTo(uploadError),
            $"Expected error '{uploadError}' but got: '{result.Error}'");

        StoragePortMock.Verify(
            x => x.UploadAsync(
                It.Is<string>(id => !string.IsNullOrEmpty(id)),
                It.Is<Stream>(s => ReferenceEquals(s, fileStream)),
                It.Is<string>(fn => fn == command.FileName),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected UploadAsync to be called once even on failure");

        RegistryPortMock.Verify(
            x => x.SaveRecording(
                It.Is<RecordingEntity>(e => e != null),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Never,
            "Expected SaveAsync to never be called when StoragePort fails (Railway pattern)");

        StoragePortMock.VerifyNoOtherCalls();
        RegistryPortMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should return failure when RegistryPort fails after successful upload")]
    public async Task ShouldReturnFailureWhenRegistryPortFails()
    {
        // Given: storage upload succeeds but registry save fails
        Stream fileStream = new MemoryStream(new byte[] { 1, 2, 3 });
        AdminAddRecordingCommand command = new(
            Name: "Taller de respiracion",
            Description: "Tecnicas avanzadas de respiracion consciente",
            Type: RecordingType.Taller,
            FileStream: fileStream,
            FileName: "respiracion.mp4");

        string storageKey = "recordings/some-id/respiracion.mp4";
        string registryError = "REGISTRY_SAVE_FAILED";

        CurrentUserServiceMock.Setup(x => x.UserId).Returns("admin-user-id");

        StoragePortMock
            .Setup(x => x.UploadAsync(It.IsAny<string>(), fileStream, command.FileName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(storageKey));

        RegistryPortMock
            .Setup(x => x.SaveRecording(It.IsAny<RecordingEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<string>(registryError));

        // When: the handler processes the command
        Result<string> result = await Handler.Handle(command, CancellationToken.None);

        // Then: result should be failure with the registry error message
        Assert.That(result.IsSuccess, Is.False,
            $"Expected failure when RegistryPort fails, but got success with value: '{(result.IsSuccess ? result.Value : "N/A")}'");
        Assert.That(result.Error, Is.EqualTo(registryError),
            $"Expected error '{registryError}' but got: '{result.Error}'");

        StoragePortMock.Verify(
            x => x.UploadAsync(
                It.Is<string>(id => !string.IsNullOrEmpty(id)),
                It.Is<Stream>(s => ReferenceEquals(s, fileStream)),
                It.Is<string>(fn => fn == command.FileName),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected UploadAsync to be called once");

        RegistryPortMock.Verify(
            x => x.SaveRecording(
                It.Is<RecordingEntity>(e =>
                    e.Name == command.Name &&
                    e.Description == command.Description &&
                    e.Type == command.Type &&
                    e.StorageKey == storageKey &&
                    !string.IsNullOrEmpty(e.Id)),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected SaveAsync to be called once even when it returns failure");

        StoragePortMock.VerifyNoOtherCalls();
        RegistryPortMock.VerifyNoOtherCalls();
    }

    #endregion

    #region Entity Mapping Tests

    [Test]
    [DisplayName("Should generate a unique recording ID for each command execution")]
    public async Task ShouldGenerateUniqueRecordingIdForEachExecution()
    {
        // Given: two identical commands
        Stream fileStream1 = new MemoryStream(new byte[] { 1, 2, 3 });
        Stream fileStream2 = new MemoryStream(new byte[] { 4, 5, 6 });
        AdminAddRecordingCommand command1 = new(
            Name: "Meditacion", Description: "Descripcion", Type: RecordingType.Meditacion,
            FileStream: fileStream1, FileName: "file.mp4");
        AdminAddRecordingCommand command2 = new(
            Name: "Meditacion", Description: "Descripcion", Type: RecordingType.Meditacion,
            FileStream: fileStream2, FileName: "file.mp4");

        CurrentUserServiceMock.Setup(x => x.UserId).Returns("admin-user-id");

        StoragePortMock
            .Setup(x => x.UploadAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success("key"));

        RegistryPortMock
            .Setup(x => x.SaveRecording(It.IsAny<RecordingEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RecordingEntity entity, CancellationToken _) => Result.Success(entity.Id));

        // When: the handler processes both commands
        Result<string> result1 = await Handler.Handle(command1, CancellationToken.None);
        Result<string> result2 = await Handler.Handle(command2, CancellationToken.None);

        // Then: each execution should produce a different ID
        Assert.That(result1.IsSuccess, Is.True,
            $"Expected first result to be success but got error: '{(result1.IsSuccess ? "N/A" : result1.Error)}'");
        Assert.That(result2.IsSuccess, Is.True,
            $"Expected second result to be success but got error: '{(result2.IsSuccess ? "N/A" : result2.Error)}'");
        Assert.That(result1.Value, Is.Not.EqualTo(result2.Value),
            $"Expected unique IDs per execution but both returned: '{result1.Value}'");
    }

    #endregion

    #region Logging Tests

    [Test]
    [DisplayName("Should log warning when StoragePort fails")]
    public async Task ShouldLogWarningWhenStoragePortFails()
    {
        // Given: storage upload fails
        Stream fileStream = new MemoryStream(new byte[] { 1, 2, 3 });
        AdminAddRecordingCommand command = new(
            Name: "Yoga nocturno",
            Description: "Sesion de yoga para relajarse",
            Type: RecordingType.Masterclass,
            FileStream: fileStream,
            FileName: "yoga.mp4");

        string uploadError = RecordingErrors.UploadFailed;
        CurrentUserServiceMock.Setup(x => x.UserId).Returns("admin-user-id");
        StoragePortMock
            .Setup(x => x.UploadAsync(It.IsAny<string>(), fileStream, command.FileName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<string>(uploadError));

        // When: the handler processes the command
        Result<string> result = await Handler.Handle(command, CancellationToken.None);

        // Then: result is failure and a Warning is logged with the error
        Assert.That(result.IsSuccess, Is.False,
            "Expected failure result when StoragePort fails");

        LoggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains(uploadError)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "Expected a Warning log containing the error message");
    }

    #endregion
}
