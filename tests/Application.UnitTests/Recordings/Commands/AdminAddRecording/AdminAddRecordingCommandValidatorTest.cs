using System.ComponentModel;
using FluentValidation.Results;
using FluentValidation.TestHelper;
using NUnit.Framework;
using VibraHeka.Application.Recordings.Commnads.AdminAddRecording;
using VibraHeka.Domain.Recordings.Enums;
using VibraHeka.Domain.Recordings.Errors;

namespace VibraHeka.Application.UnitTests.Recordings.Commands.AdminAddRecording;

[TestFixture]
public class AdminAddRecordingCommandValidatorTest
{
    private AdminAddRecordingCommandValidator Validator;

    [SetUp]
    public void SetUp()
    {
        Validator = new AdminAddRecordingCommandValidator();
    }

    #region Name Validation Tests

    [TestCase("", TestName = "Empty name")]
    [TestCase("   ", TestName = "Whitespace name")]
    [DisplayName("Should fail when Name is empty or whitespace")]
    public void ShouldFailValidationWhenNameIsEmptyOrWhitespace(string name)
    {
        // Given: a command with an empty or whitespace Name
        AdminAddRecordingCommand command = BuildValidCommand() with { Name = name };

        // When: validating the command
        TestValidationResult<AdminAddRecordingCommand> result = Validator.TestValidate(command);

        // Then: should have exactly one validation error for Name with InvalidName error
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(RecordingErrors.InvalidName);
    }

    [Test]
    [DisplayName("Should fail when Name exceeds 200 characters")]
    public void ShouldFailValidationWhenNameExceeds200Characters()
    {
        // Given: a command with a Name of 201 characters
        string longName = new string('a', 201);
        AdminAddRecordingCommand command = BuildValidCommand() with { Name = longName };

        // When: validating the command
        TestValidationResult<AdminAddRecordingCommand> result = Validator.TestValidate(command);

        // Then: should have a validation error for Name
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(RecordingErrors.InvalidName);
    }

    [Test]
    [DisplayName("Should pass when Name has exactly 200 characters")]
    public void ShouldPassValidationWhenNameHasExactly200Characters()
    {
        // Given: a command with a Name of exactly 200 characters
        string maxName = new string('a', 200);
        AdminAddRecordingCommand command = BuildValidCommand() with { Name = maxName };

        // When: validating the command
        TestValidationResult<AdminAddRecordingCommand> result = Validator.TestValidate(command);

        // Then: should not have any validation error for Name
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    [DisplayName("Should stop at first Name error when CascadeMode is Stop")]
    public void ShouldStopAtFirstNameErrorWhenCascadeModeIsStop()
    {
        // Given: a command with an empty Name (triggers both NotEmpty and MaximumLength rules)
        AdminAddRecordingCommand command = BuildValidCommand() with { Name = "" };

        // When: validating the command
        TestValidationResult<AdminAddRecordingCommand> result = Validator.TestValidate(command);

        // Then: should have exactly 1 error for Name, not multiple
        IEnumerable<ValidationFailure> nameErrors = result.Errors
            .Where(e => e.PropertyName == nameof(AdminAddRecordingCommand.Name))
            .ToList();

        Assert.That(nameErrors.Count(), Is.EqualTo(1),
            $"Expected exactly 1 error for Name with CascadeMode.Stop, but got {nameErrors.Count()}");
        Assert.That(nameErrors.First().ErrorMessage, Is.EqualTo(RecordingErrors.InvalidName),
            $"Expected error '{RecordingErrors.InvalidName}' but got: '{nameErrors.First().ErrorMessage}'");
    }

    #endregion

    #region Description Validation Tests

    [TestCase("", TestName = "Empty description")]
    [TestCase("   ", TestName = "Whitespace description")]
    [DisplayName("Should fail when Description is empty or whitespace")]
    public void ShouldFailValidationWhenDescriptionIsEmptyOrWhitespace(string description)
    {
        // Given: a command with an empty or whitespace Description
        AdminAddRecordingCommand command = BuildValidCommand() with { Description = description };

        // When: validating the command
        TestValidationResult<AdminAddRecordingCommand> result = Validator.TestValidate(command);

        // Then: should have a validation error for Description with InvalidDescription error
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage(RecordingErrors.InvalidDescription);
    }

    [Test]
    [DisplayName("Should fail when Description exceeds 2000 characters")]
    public void ShouldFailValidationWhenDescriptionExceeds2000Characters()
    {
        // Given: a command with a Description of 2001 characters
        string longDescription = new string('b', 2001);
        AdminAddRecordingCommand command = BuildValidCommand() with { Description = longDescription };

        // When: validating the command
        TestValidationResult<AdminAddRecordingCommand> result = Validator.TestValidate(command);

        // Then: should have a validation error for Description
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage(RecordingErrors.InvalidDescription);
    }

    [Test]
    [DisplayName("Should pass when Description has exactly 2000 characters")]
    public void ShouldPassValidationWhenDescriptionHasExactly2000Characters()
    {
        // Given: a command with a Description of exactly 2000 characters
        string maxDescription = new string('b', 2000);
        AdminAddRecordingCommand command = BuildValidCommand() with { Description = maxDescription };

        // When: validating the command
        TestValidationResult<AdminAddRecordingCommand> result = Validator.TestValidate(command);

        // Then: should not have any validation error for Description
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Test]
    [DisplayName("Should stop at first Description error when CascadeMode is Stop")]
    public void ShouldStopAtFirstDescriptionErrorWhenCascadeModeIsStop()
    {
        // Given: a command with an empty Description
        AdminAddRecordingCommand command = BuildValidCommand() with { Description = "" };

        // When: validating the command
        TestValidationResult<AdminAddRecordingCommand> result = Validator.TestValidate(command);

        // Then: should have exactly 1 error for Description
        IEnumerable<ValidationFailure> descErrors = result.Errors
            .Where(e => e.PropertyName == nameof(AdminAddRecordingCommand.Description))
            .ToList();

        Assert.That(descErrors.Count(), Is.EqualTo(1),
            $"Expected exactly 1 error for Description with CascadeMode.Stop, but got {descErrors.Count()}");
        Assert.That(descErrors.First().ErrorMessage, Is.EqualTo(RecordingErrors.InvalidDescription),
            $"Expected error '{RecordingErrors.InvalidDescription}' but got: '{descErrors.First().ErrorMessage}'");
    }

    #endregion

    #region Type Validation Tests

    [Test]
    [DisplayName("Should fail when Type is outside enum range")]
    public void ShouldFailValidationWhenTypeIsOutsideEnumRange()
    {
        // Given: a command with a Type value not defined in RecordingType enum
        AdminAddRecordingCommand command = BuildValidCommand() with { Type = (RecordingType)999 };

        // When: validating the command
        TestValidationResult<AdminAddRecordingCommand> result = Validator.TestValidate(command);

        // Then: should have a validation error for Type with InvalidType error
        result.ShouldHaveValidationErrorFor(x => x.Type)
            .WithErrorMessage(RecordingErrors.InvalidType);
    }

    [TestCase(RecordingType.Meditacion, TestName = "Meditacion")]
    [TestCase(RecordingType.Masterclass, TestName = "Masterclass")]
    [TestCase(RecordingType.Taller, TestName = "Taller")]
    [DisplayName("Should pass when Type is a valid enum value")]
    public void ShouldPassValidationWhenTypeIsValidEnumValue(RecordingType type)
    {
        // Given: a command with a valid RecordingType value
        AdminAddRecordingCommand command = BuildValidCommand() with { Type = type };

        // When: validating the command
        TestValidationResult<AdminAddRecordingCommand> result = Validator.TestValidate(command);

        // Then: should not have any validation error for Type
        result.ShouldNotHaveValidationErrorFor(x => x.Type);
    }

    #endregion

    #region FileStream Validation Tests

    [Test]
    [DisplayName("Should fail when FileStream is null")]
    public void ShouldFailValidationWhenFileStreamIsNull()
    {
        // Given: a command with a null FileStream
        AdminAddRecordingCommand command = BuildValidCommand() with { FileStream = null! };

        // When: validating the command
        TestValidationResult<AdminAddRecordingCommand> result = Validator.TestValidate(command);

        // Then: should have a validation error for FileStream with InvalidFile error
        result.ShouldHaveValidationErrorFor(x => x.FileStream)
            .WithErrorMessage(RecordingErrors.InvalidFile);
    }

    [Test]
    [DisplayName("Should fail when FileStream length is zero")]
    public void ShouldFailValidationWhenFileStreamLengthIsZero()
    {
        // Given: a command with an empty FileStream (length == 0)
        Stream emptyStream = new MemoryStream(Array.Empty<byte>());
        AdminAddRecordingCommand command = BuildValidCommand() with { FileStream = emptyStream };

        // When: validating the command
        TestValidationResult<AdminAddRecordingCommand> result = Validator.TestValidate(command);

        // Then: should have a validation error for FileStream with InvalidFile error
        result.ShouldHaveValidationErrorFor(x => x.FileStream)
            .WithErrorMessage(RecordingErrors.InvalidFile);
    }

    [Test]
    [DisplayName("Should stop at first FileStream error when FileStream is null (CascadeMode Stop)")]
    public void ShouldStopAtFirstFileStreamErrorWhenFileStreamIsNullAndCascadeModeIsStop()
    {
        // Given: a command with null FileStream (both NotNull and Must rules would fire without cascade stop)
        AdminAddRecordingCommand command = BuildValidCommand() with { FileStream = null! };

        // When: validating the command
        TestValidationResult<AdminAddRecordingCommand> result = Validator.TestValidate(command);

        // Then: should have exactly 1 error for FileStream
        IEnumerable<ValidationFailure> streamErrors = result.Errors
            .Where(e => e.PropertyName == nameof(AdminAddRecordingCommand.FileStream))
            .ToList();

        Assert.That(streamErrors.Count(), Is.EqualTo(1),
            $"Expected exactly 1 error for FileStream with CascadeMode.Stop, but got {streamErrors.Count()}");
        Assert.That(streamErrors.First().ErrorMessage, Is.EqualTo(RecordingErrors.InvalidFile),
            $"Expected error '{RecordingErrors.InvalidFile}' but got: '{streamErrors.First().ErrorMessage}'");
    }

    #endregion

    #region FileName Validation Tests

    [TestCase("", TestName = "Empty FileName")]
    [TestCase("   ", TestName = "Whitespace FileName")]
    [DisplayName("Should fail when FileName is empty or whitespace")]
    public void ShouldFailValidationWhenFileNameIsEmptyOrWhitespace(string fileName)
    {
        // Given: a command with an empty or whitespace FileName
        AdminAddRecordingCommand command = BuildValidCommand() with { FileName = fileName };

        // When: validating the command
        TestValidationResult<AdminAddRecordingCommand> result = Validator.TestValidate(command);

        // Then: should have a validation error for FileName with InvalidFile error
        result.ShouldHaveValidationErrorFor(x => x.FileName)
            .WithErrorMessage(RecordingErrors.InvalidFile);
    }

    #endregion

    #region Full Command Validation Tests

    [Test]
    [DisplayName("Should pass when all fields are valid")]
    public void ShouldPassValidationWhenAllFieldsAreValid()
    {
        // Given: a command with all fields valid
        AdminAddRecordingCommand command = BuildValidCommand();

        // When: validating the command
        TestValidationResult<AdminAddRecordingCommand> result = Validator.TestValidate(command);

        // Then: should not have any validation errors
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion

    #region Helpers

    private static AdminAddRecordingCommand BuildValidCommand() =>
        AdminAddRecordingCommandBuilder.BuildValid();

    #endregion
}

