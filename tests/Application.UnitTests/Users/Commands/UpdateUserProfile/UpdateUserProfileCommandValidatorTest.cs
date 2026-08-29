using FluentValidation.TestHelper;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Users.Commands.UpdateUserProfile;
namespace VibraHeka.Application.UnitTests.Users.Commands.UpdateUserProfile;

[TestFixture]
public class UpdateUserProfileCommandValidatorTest
{
    private UpdateUserProfileCommandValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new UpdateUserProfileCommandValidator();
    }

    [Test]
    public void ShouldPassValidationWhenCommandIsValid()
    {
        // Given
        UpdateUserProfileCommand command = new(
            Guid.NewGuid().ToString(),
            "valid@test.com",
            "John",
            "M",
            "Doe",
            "Bio",
            "https://example.com/avatar.jpg",
            "+34911111222"
        );

        // When
        TestValidationResult<UpdateUserProfileCommand> result = _validator.TestValidate(command);

        // Then
        result.ShouldNotHaveAnyValidationErrors();
    }


    [Test]
    public void ShouldFailWhenEmailIsInvalid()
    {
        // Given
        UpdateUserProfileCommand command = new(
            "invalid-email",
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty
        );

        // When
        TestValidationResult<UpdateUserProfileCommand> result = _validator.TestValidate(command);

        // Then
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorCode(UserErrors.InvalidEmail);
    }
}
