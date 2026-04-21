using System.ComponentModel;
using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Common.Behaviours;
using VibraHeka.Application.Recordings.Commnads.AdminAddRecording;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Recordings.Entities;
using VibraHeka.Domain.Recordings.Enums;
using VibraHeka.Domain.Recordings.Ports.Out;

namespace VibraHeka.Application.FunctionalTests.Recordings.Commands.AdminAddRecording;

[TestFixture]
public class AdminAddRecordingCommandHandlerTest
{
    private Mock<IRecordingStoragePort> StoragePortMock = default!;
    private Mock<IRecordingRegistryPort> RegistryPortMock = default!;
    private Mock<ICurrentUserService> CurrentUserServiceMock = default!;
    private Mock<ILogger<AdminAddRecordingCommandHandler>> LoggerMock = default!;
    private AdminAddRecordingCommandHandler Handler = default!;
    private ValidationBehaviour<AdminAddRecordingCommand, Result<string>> Pipeline = default!;

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

        Pipeline = new ValidationBehaviour<AdminAddRecordingCommand, Result<string>>(
            [new AdminAddRecordingCommandValidator()]);
    }

    [Test]
    [DisplayName("Pipeline happy path: validation passes, handler executes, returns success")]
    public async Task ShouldReturnSuccessWhenPipelineCompletesWithValidCommand()
    {
        // Given: a valid command with storage and registry mocks succeeding
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
            .Setup(x => x.SaveAsync(It.IsAny<RecordingEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RecordingEntity entity, CancellationToken _) => Result.Success(entity.Id));

        // When: the ValidationBehaviour + handler pipeline processes the command
        Result<string> result = await Pipeline.Handle(
            command,
            ct => Handler.Handle(command, ct),
            CancellationToken.None);

        // Then: the result should be success with a non-empty recording ID
        Assert.That(result.IsSuccess, Is.True,
            $"Expected pipeline to succeed but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");
        Assert.That(result.Value, Is.Not.Null.And.Not.Empty,
            "Expected a non-empty recording ID from the pipeline but got null or empty");

        StoragePortMock.Verify(
            x => x.UploadAsync(
                It.Is<string>(id => !string.IsNullOrEmpty(id)),
                It.Is<Stream>(s => ReferenceEquals(s, fileStream)),
                It.Is<string>(fn => fn == command.FileName),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected UploadAsync to be called exactly once through the full pipeline");

        RegistryPortMock.Verify(
            x => x.SaveAsync(
                It.Is<RecordingEntity>(e =>
                    e.Name == command.Name &&
                    e.Description == command.Description &&
                    e.Type == command.Type &&
                    e.StorageKey == storageKey &&
                    e.CreatedBy == adminUserId &&
                    !string.IsNullOrEmpty(e.Id)),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected SaveAsync to be called once with correctly mapped entity");

        StoragePortMock.VerifyNoOtherCalls();
        RegistryPortMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Pipeline with empty Name: ValidationBehaviour throws before handler — storage/registry never called")]
    public void ShouldThrowValidationExceptionAndNotCallPortsWhenNameIsEmpty()
    {
        // Given: a command with an empty Name that fails validation
        AdminAddRecordingCommand command = AdminAddRecordingCommandBuilder.BuildValid() with { Name = "" };

        // When: the pipeline processes the invalid command
        AsyncTestDelegate action = async () => await Pipeline.Handle(
            command,
            ct => Handler.Handle(command, ct),
            CancellationToken.None);

        // Then: ValidationBehaviour should throw before handler runs
        Assert.That(action, Throws.TypeOf<ValidationException>(),
            "Expected ValidationException to be thrown by ValidationBehaviour when Name is empty");

        StoragePortMock.Verify(
            x => x.UploadAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "Expected UploadAsync to never be called when validation fails on Name");

        RegistryPortMock.Verify(
            x => x.SaveAsync(It.IsAny<RecordingEntity>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "Expected SaveAsync to never be called when validation fails on Name");

        StoragePortMock.VerifyNoOtherCalls();
        RegistryPortMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Pipeline with null FileStream: ValidationBehaviour throws before handler — storage/registry never called")]
    public void ShouldThrowValidationExceptionAndNotCallPortsWhenFileStreamIsNull()
    {
        // Given: a command with a null FileStream that fails validation
        AdminAddRecordingCommand command = AdminAddRecordingCommandBuilder.BuildValid() with { FileStream = null! };

        // When: the pipeline processes the invalid command
        AsyncTestDelegate action = async () => await Pipeline.Handle(
            command,
            ct => Handler.Handle(command, ct),
            CancellationToken.None);

        // Then: ValidationBehaviour should throw before handler runs
        Assert.That(action, Throws.TypeOf<ValidationException>(),
            "Expected ValidationException to be thrown by ValidationBehaviour when FileStream is null");

        StoragePortMock.Verify(
            x => x.UploadAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "Expected UploadAsync to never be called when validation fails on FileStream");

        RegistryPortMock.Verify(
            x => x.SaveAsync(It.IsAny<RecordingEntity>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "Expected SaveAsync to never be called when validation fails on FileStream");

        StoragePortMock.VerifyNoOtherCalls();
        RegistryPortMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Pipeline with invalid Type: ValidationBehaviour throws before handler — storage/registry never called")]
    public void ShouldThrowValidationExceptionAndNotCallPortsWhenTypeIsInvalid()
    {
        // Given: a command with an invalid RecordingType that fails validation
        AdminAddRecordingCommand command = AdminAddRecordingCommandBuilder.BuildValid() with { Type = (RecordingType)999 };

        // When: the pipeline processes the invalid command
        AsyncTestDelegate action = async () => await Pipeline.Handle(
            command,
            ct => Handler.Handle(command, ct),
            CancellationToken.None);

        // Then: ValidationBehaviour should throw before handler runs
        Assert.That(action, Throws.TypeOf<ValidationException>(),
            "Expected ValidationException to be thrown by ValidationBehaviour when Type is (RecordingType)999");

        StoragePortMock.Verify(
            x => x.UploadAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "Expected UploadAsync to never be called when validation fails on Type");

        RegistryPortMock.Verify(
            x => x.SaveAsync(It.IsAny<RecordingEntity>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "Expected SaveAsync to never be called when validation fails on Type");

        StoragePortMock.VerifyNoOtherCalls();
        RegistryPortMock.VerifyNoOtherCalls();
    }
}
