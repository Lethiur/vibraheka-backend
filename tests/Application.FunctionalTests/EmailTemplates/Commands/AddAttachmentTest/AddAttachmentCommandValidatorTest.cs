using FluentValidation.Results;
using NUnit.Framework;
using VibraHeka.Application.EmailTemplates.Commands.AddAttachment;
using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Application.FunctionalTests.EmailTemplates.Commands.AddAttachmentTest;

[TestFixture]
public class AddAttachmentCommandValidatorTests
{
    private AddAttachmentCommandValidator Validator;

    [SetUp]
    public void SetUp()
    {
        Validator = new AddAttachmentCommandValidator();
    }

    public static IEnumerable<TestCaseData> ValidMediaCases()
    {
        yield return new TestCaseData(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00 })
            .SetName("ShouldPassValidationWhenStreamIsJpeg");
        yield return new TestCaseData(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A })
            .SetName("ShouldPassValidationWhenStreamIsPng");
        yield return new TestCaseData(new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 })
            .SetName("ShouldPassValidationWhenStreamIsGif");
        yield return new TestCaseData(new byte[] { 0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50 })
            .SetName("ShouldPassValidationWhenStreamIsWebp");
        yield return new TestCaseData(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x66, 0x74, 0x79, 0x70, 0x6D, 0x70, 0x34, 0x32 })
            .SetName("ShouldPassValidationWhenStreamIsMp4");
        yield return new TestCaseData(new byte[] { 0x1A, 0x45, 0xDF, 0xA3, 0x93, 0x42 })
            .SetName("ShouldPassValidationWhenStreamIsMkv");
    }

    public static IEnumerable<TestCaseData> InvalidMediaCases()
    {
        yield return new TestCaseData(new byte[] { 0x00, 0x11, 0x22, 0x33, 0x44 })
            .SetName("ShouldFailValidationWhenStreamIsUnknownBinary");
        yield return new TestCaseData(new byte[] { 0x7B, 0x22, 0x6A, 0x73, 0x6F, 0x6E, 0x22, 0x3A, 0x74, 0x72, 0x75, 0x65, 0x7D })
            .SetName("ShouldFailValidationWhenStreamIsJsonText");
    }

    [Test]
    [Description("Given a command with null file stream, when validating, then it should fail with InvalidAttachmentContent error")]
    public async Task ShouldFailValidationWhenFileStreamIsNull()
    {
        // Given: a null stream to verify validation rejects missing files.
        AddAttachmentCommand command = new AddAttachmentCommand(null!, "template-123", "file.png");

        // When: validating the command.
        ValidationResult result = await Validator.ValidateAsync(command);

        // Then
        Assert.That(result.IsValid, Is.False);
        Assert.That(
            result.Errors,
            Has.Some.Matches<ValidationFailure>(e => e.ErrorMessage == EmailTemplateErrors.InvalidAttachmentContent)
        );
    }

    [Test]
    [Description("Given a command with empty file stream, when validating, then it should fail with InvalidAttachmentContent error")]
    public async Task ShouldFailValidationWhenFileStreamIsEmpty()
    {
        // Given: an empty stream to verify validation rejects empty files.
        AddAttachmentCommand command = new AddAttachmentCommand(new MemoryStream(Array.Empty<byte>()), "template-123", "file.png");

        // When: validating the command.
        ValidationResult result = await Validator.ValidateAsync(command);

        // Then
        Assert.That(result.IsValid, Is.False);
        Assert.That(
            result.Errors,
            Has.Some.Matches<ValidationFailure>(e => e.ErrorMessage == EmailTemplateErrors.InvalidAttachmentContent)
        );
    }

    [TestCaseSource(nameof(ValidMediaCases))]
    [Description("Given a command with valid image or video stream, when validating, then it should pass validation")]
    public async Task ShouldPassValidationWhenMediaIsValid(byte[] bytes)
    {
        // Given: a valid media header to verify validation accepts it.
        AddAttachmentCommand command = new AddAttachmentCommand(new MemoryStream(bytes), "template-123", "file.bin");

        // When: validating the command.
        ValidationResult result = await Validator.ValidateAsync(command);

        // Then
        Assert.That(result.IsValid);
        Assert.That(result.Errors, Is.Empty);
    }

    [TestCaseSource(nameof(InvalidMediaCases))]
    [Description("Given a command with invalid stream, when validating, then it should fail with InvalidAttachmentContent error")]
    public async Task ShouldFailValidationWhenMediaIsInvalid(byte[] bytes)
    {
        // Given: a non-media stream to verify validation rejects it.
        AddAttachmentCommand command = new AddAttachmentCommand(new MemoryStream(bytes), "template-123", "file.bin");

        // When: validating the command.
        ValidationResult result = await Validator.ValidateAsync(command);

        // Then
        Assert.That(result.IsValid, Is.False);
        Assert.That(
            result.Errors,
            Has.Some.Matches<ValidationFailure>(e => e.ErrorMessage == EmailTemplateErrors.InvalidAttachmentContent)
        );
    }

    [Test]
    [Description("Given a command with valid media stream, when validating, then the stream position should be restored")]
    public async Task ShouldRestoreStreamPositionAfterValidation()
    {
        // Given: a valid media stream with a non-zero position to verify the position is restored.
        MemoryStream stream = new MemoryStream(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00 });
        stream.Position = 4;
        AddAttachmentCommand command = new AddAttachmentCommand(stream, "template-123", "file.png");

        // When: validating the command.
        ValidationResult result = await Validator.ValidateAsync(command);

        // Then
        Assert.That(result.IsValid);
        Assert.That(stream.Position, Is.EqualTo(4));
    }
}
