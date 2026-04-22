using System.ComponentModel;
using FluentValidation.Results;
using FluentValidation.TestHelper;
using NUnit.Framework;
using VibraHeka.Application.Recordings.Commnads.AdminAddRecording;
using VibraHeka.Domain.Recordings.Errors;

namespace VibraHeka.Application.UnitTests.Recordings.Commands.AdminAddRecording;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class AdminAddRecordingCommandValidatorTest : GenericAdminAddRecordingTest
{
    private AdminAddRecordingCommandValidator Validator = default!;

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        Validator = new AdminAddRecordingCommandValidator();
    }

    #region Name Validation

    [TestCase("", TestName = "Empty name")]
    [TestCase("   ", TestName = "Whitespace name")]
    [DisplayName("Should fail validation when Name is empty or whitespace")]
    public void ShouldFailValidationWhenNameIsEmptyOrWhitespace(string name)
    {
        // Given: a command with an empty or whitespace name
        AdminAddRecordingCommand command = new(
            Name: name,
            Description: "Descripcion valida",
            Type: Domain.Recordings.Enums.RecordingType.Meditacion,
            FileName: "file.mp4");

        // When: validating the command
        TestValidationResult<AdminAddRecordingCommand> result = Validator.TestValidate(command);

        // Then: should have a validation error for Name with InvalidName error code
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(RecordingErrors.InvalidName);
    }

    [Test]
    [DisplayName("Should fail validation when Name exceeds 200 characters")]
    public void ShouldFailValidationWhenNameExceeds200Characters()
    {
        // Given: a command with a name longer than 200 characters
        AdminAddRecordingCommand command = new(
            Name: new string('A', 201),
            Description: "Descripcion valida",
            Type: Domain.Recordings.Enums.RecordingType.Meditacion,
            FileName: "file.mp4");

        // When: validating the command
        TestValidationResult<AdminAddRecordingCommand> result = Validator.TestValidate(command);

        // Then: should have a validation error for Name with InvalidName error code
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(RecordingErrors.InvalidName);
    }

    #endregion

    #region Description Validation

    [TestCase("", TestName = "Empty description")]
    [TestCase("   ", TestName = "Whitespace description")]
    [DisplayName("Should fail validation when Description is empty or whitespace")]
    public void ShouldFailValidationWhenDescriptionIsEmptyOrWhitespace(string description)
    {
        // Given: a command with an empty or whitespace description
        AdminAddRecordingCommand command = new(
            Name: "Nombre valido",
            Description: description,
            Type: Domain.Recordings.Enums.RecordingType.Meditacion,
            FileName: "file.mp4");

        // When: validating the command
        TestValidationResult<AdminAddRecordingCommand> result = Validator.TestValidate(command);

        // Then: should have a validation error for Description with InvalidDescription error code
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage(RecordingErrors.InvalidDescription);
    }

    [Test]
    [DisplayName("Should fail validation when Description exceeds 2000 characters")]
    public void ShouldFailValidationWhenDescriptionExceeds2000Characters()
    {
        // Given: a command with a description longer than 2000 characters
        AdminAddRecordingCommand command = new(
            Name: "Nombre valido",
            Description: new string('D', 2001),
            Type: Domain.Recordings.Enums.RecordingType.Meditacion,
            FileName: "file.mp4");

        // When: validating the command
        TestValidationResult<AdminAddRecordingCommand> result = Validator.TestValidate(command);

        // Then: should have a validation error for Description with InvalidDescription error code
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage(RecordingErrors.InvalidDescription);
    }

    #endregion

    #region Type Validation

    [Test]
    [DisplayName("Should fail validation when Type is not a valid enum value")]
    public void ShouldFailValidationWhenTypeIsNotAValidEnumValue()
    {
        // Given: a command with an invalid enum value for Type
        AdminAddRecordingCommand command = new(
            Name: "Nombre valido",
            Description: "Descripcion valida",
            Type: (Domain.Recordings.Enums.RecordingType)999,
            FileName: "file.mp4");

        // When: validating the command
        TestValidationResult<AdminAddRecordingCommand> result = Validator.TestValidate(command);

        // Then: should have a validation error for Type with InvalidType error code
        result.ShouldHaveValidationErrorFor(x => x.Type)
            .WithErrorMessage(RecordingErrors.InvalidType);
    }

    #endregion

    #region FileName Validation

    [TestCase("", TestName = "Empty FileName")]
    [TestCase("   ", TestName = "Whitespace FileName")]
    [DisplayName("Should fail validation when FileName is empty or whitespace")]
    public void ShouldFailValidationWhenFileNameIsEmptyOrWhitespace(string fileName)
    {
        // Given: a command with an empty or whitespace FileName
        AdminAddRecordingCommand command = new(
            Name: "Nombre valido",
            Description: "Descripcion valida",
            Type: Domain.Recordings.Enums.RecordingType.Meditacion,
            FileName: fileName);

        // When: validating the command
        TestValidationResult<AdminAddRecordingCommand> result = Validator.TestValidate(command);

        // Then: should have a validation error for FileName with InvalidFile error code
        result.ShouldHaveValidationErrorFor(x => x.FileName)
            .WithErrorMessage(RecordingErrors.InvalidFile);
    }

    [TestCase("video", TestName = "No extension")]
    [TestCase("video/name.mp4", TestName = "Contains slash")]
    [TestCase("video\\name.mp4", TestName = "Contains backslash")]
    [DisplayName("Should fail validation when FileName has invalid format")]
    public void ShouldFailValidationWhenFileNameHasInvalidFormat(string fileName)
    {
        // Given: a command with a FileName that does not match the required pattern
        AdminAddRecordingCommand command = new(
            Name: "Nombre valido",
            Description: "Descripcion valida",
            Type: Domain.Recordings.Enums.RecordingType.Meditacion,
            FileName: fileName);

        // When: validating the command
        TestValidationResult<AdminAddRecordingCommand> result = Validator.TestValidate(command);

        // Then: should have a validation error for FileName with InvalidFile error code
        result.ShouldHaveValidationErrorFor(x => x.FileName)
            .WithErrorMessage(RecordingErrors.InvalidFile);
    }

    #endregion

    #region Happy Path

    [Test]
    [DisplayName("Should pass validation when all fields are valid")]
    public void ShouldPassValidationWhenAllFieldsAreValid()
    {
        // Given: a command with all valid fields
        AdminAddRecordingCommand command = BuildValidCommand();

        // When: validating the command
        TestValidationResult<AdminAddRecordingCommand> result = Validator.TestValidate(command);

        // Then: should not have any validation errors
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion

    #region CascadeMode Tests

    [Test]
    [DisplayName("Should report exactly one error per field when CascadeMode is Stop (empty Name triggers NotEmpty only)")]
    public void ShouldReportExactlyOneErrorForNameWhenCascadeModeIsStop()
    {
        // Given: a command with an empty Name (both NotEmpty and MaximumLength would fire without cascade stop)
        AdminAddRecordingCommand command = new(
            Name: "",
            Description: "Descripcion valida",
            Type: Domain.Recordings.Enums.RecordingType.Meditacion,
            FileName: "file.mp4");

        // When: validating the command
        TestValidationResult<AdminAddRecordingCommand> result = Validator.TestValidate(command);

        // Then: should have exactly 1 error for Name (CascadeMode.Stop stops after first rule failure)
        IEnumerable<ValidationFailure> nameErrors = result.Errors
            .Where(e => e.PropertyName == nameof(AdminAddRecordingCommand.Name))
            .ToList();

        Assert.That(nameErrors.Count(), Is.EqualTo(1),
            $"Expected exactly 1 error for Name with CascadeMode.Stop, but got {nameErrors.Count()}");
        Assert.That(nameErrors.First().ErrorMessage, Is.EqualTo(RecordingErrors.InvalidName),
            $"Expected error '{RecordingErrors.InvalidName}' but got: '{nameErrors.First().ErrorMessage}'");
    }

    #endregion
}


