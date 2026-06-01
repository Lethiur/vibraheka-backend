using System.ComponentModel;
using FluentValidation.Results;
using FluentValidation.TestHelper;
using NUnit.Framework;
using VibraHeka.Application.Recordings.Commnands.DeleteRecording;
using VibraHeka.Domain.Recordings.Errors;

namespace VibraHeka.Application.UnitTests.Catalog.Commands.DeleteRecording;

[TestFixture]
public sealed class DeleteRecordingCommandValidatorTest : GenericDeleteRecordingTest
{
    private DeleteRecordingCommandValidator Validator = default!;

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        Validator = new DeleteRecordingCommandValidator();
    }

    #region RecordingId Empty / Null Validation Tests

    [TestCase("", TestName = "Empty RecordingId")]
    [TestCase("   ", TestName = "Whitespace RecordingId")]
    [DisplayName("Should fail validation when RecordingId is empty or whitespace")]
    public void ShouldFailValidationWhenRecordingIdIsEmptyOrWhitespace(string recordingId)
    {
        // Given: a command with an empty or whitespace RecordingId
        DeleteRecordingCommand command = new(RecordingId: recordingId);

        // When: validating the command
        TestValidationResult<DeleteRecordingCommand> result = Validator.TestValidate(command);

        // Then: should have a validation error for RecordingId with InvalidRecordingId error
        result.ShouldHaveValidationErrorFor(x => x.RecordingId)
            .WithErrorMessage(RecordingErrors.InvalidRecordingId);
    }

    [Test]
    [DisplayName("Should fail validation when RecordingId is null")]
    public void ShouldFailValidationWhenRecordingIdIsNull()
    {
        // Given: a command with a null RecordingId
        DeleteRecordingCommand command = new(RecordingId: null!);

        // When: validating the command
        TestValidationResult<DeleteRecordingCommand> result = Validator.TestValidate(command);

        // Then: should have a validation error for RecordingId with InvalidRecordingId error
        result.ShouldHaveValidationErrorFor(x => x.RecordingId)
            .WithErrorMessage(RecordingErrors.InvalidRecordingId);
    }

    #endregion

    #region RecordingId Format Validation Tests

    [TestCase("abc", TestName = "Plain string")]
    [TestCase("not-a-guid", TestName = "Hyphenated non-GUID")]
    [TestCase("12345678", TestName = "Short numeric string")]
    [DisplayName("Should fail validation when RecordingId is not a valid GUID format")]
    public void ShouldFailValidationWhenRecordingIdIsNotAValidGuid(string recordingId)
    {
        // Given: a command with a non-GUID RecordingId
        DeleteRecordingCommand command = new(RecordingId: recordingId);

        // When: validating the command
        TestValidationResult<DeleteRecordingCommand> result = Validator.TestValidate(command);

        // Then: should have a validation error for RecordingId with InvalidRecordingId error
        result.ShouldHaveValidationErrorFor(x => x.RecordingId)
            .WithErrorMessage(RecordingErrors.InvalidRecordingId);
    }

    [Test]
    [DisplayName("Should pass validation when RecordingId is a valid GUID")]
    public void ShouldPassValidationWhenRecordingIdIsValidGuid()
    {
        // Given: a command with a valid GUID RecordingId
        DeleteRecordingCommand command = BuildValidCommand();

        // When: validating the command
        TestValidationResult<DeleteRecordingCommand> result = Validator.TestValidate(command);

        // Then: should not have any validation errors
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion

    #region CascadeMode Tests

    [Test]
    [DisplayName("Should stop at first RecordingId error when CascadeMode is Stop (empty triggers NotEmpty, not Must)")]
    public void ShouldStopAtFirstRecordingIdErrorWhenCascadeModeIsStop()
    {
        // Given: a command with an empty RecordingId (both NotEmpty and Must rules would fire without cascade stop)
        DeleteRecordingCommand command = new(RecordingId: "");

        // When: validating the command
        TestValidationResult<DeleteRecordingCommand> result = Validator.TestValidate(command);

        // Then: should have exactly 1 error for RecordingId (CascadeMode.Stop prevents the second Must rule from firing)
        IEnumerable<ValidationFailure> idErrors = result.Errors
            .Where(e => e.PropertyName == nameof(DeleteRecordingCommand.RecordingId))
            .ToList();

        Assert.That(
            idErrors.Count(),
            Is.EqualTo(1),
            $"Expected exactly 1 error for RecordingId with CascadeMode.Stop, but got {idErrors.Count()}");

        Assert.That(
            idErrors.First().ErrorMessage,
            Is.EqualTo(RecordingErrors.InvalidRecordingId),
            $"Expected error '{RecordingErrors.InvalidRecordingId}' but got: '{idErrors.First().ErrorMessage}'");
    }

    #endregion
}

