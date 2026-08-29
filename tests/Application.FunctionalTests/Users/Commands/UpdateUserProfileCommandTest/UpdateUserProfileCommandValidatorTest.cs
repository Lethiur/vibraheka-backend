using FluentValidation.Results;
using NUnit.Framework;
using VibraHeka.Application.Users.Commands.UpdateUserProfile;

namespace VibraHeka.Application.FunctionalTests.Users.Commands.UpdateUserProfileCommandTest;

[TestFixture]
public class UpdateUserProfileCommandValidatorTest
{
    private UpdateUserProfileCommandValidator _validator = default!;

    [SetUp]
    public void SetUp()
    {
        _validator = new UpdateUserProfileCommandValidator();
    }

    [Test]
    public async Task ShouldPassValidationWhenDataIsValid()
    {
        // Given
        UpdateUserProfileCommand command = new(
        
            
             "valid@test.com",
             "John",
                "Middle",
                "Doe",
                "+34911111222",
                "Bio",
                string.Empty,
                "Europe/Madrid"
        );

        // When
        ValidationResult result = await _validator.ValidateAsync(command);

        // Then
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public async Task ShouldFailValidationWhenEmailIsInvalid()
    {
        // Given
        UpdateUserProfileCommand command = new(
            "invalid-email",
            "John",
            "Middle",
            "Doe",
            "+34911111222",
            "Bio",
            string.Empty,
            "Europe/Madrid"
        );

        // When
        ValidationResult result = await _validator.ValidateAsync(command);

        // Then
        Assert.That(result.IsValid, Is.False);
    }
}

