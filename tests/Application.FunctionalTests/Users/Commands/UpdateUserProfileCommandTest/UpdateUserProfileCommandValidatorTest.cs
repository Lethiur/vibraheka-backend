using NUnit.Framework;
using FluentValidation.Results;
using VibraHeka.Application.Users.Commands.UpdateUserProfile;
using VibraHeka.Domain.Models.Results.User;

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
        UpdateUserProfileCommand command = new(new UserDTO
        {
            Id = Guid.NewGuid().ToString(),
            Email = "valid@test.com",
            FirstName = "John"
        });

        // When
        ValidationResult result = await _validator.ValidateAsync(command);

        // Then
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public async Task ShouldFailValidationWhenEmailIsInvalid()
    {
        // Given
        UpdateUserProfileCommand command = new(new UserDTO
        {
            Id = Guid.NewGuid().ToString(),
            Email = "invalid-email"
        });

        // When
        ValidationResult result = await _validator.ValidateAsync(command);

        // Then
        Assert.That(result.IsValid, Is.False);
    }
}

