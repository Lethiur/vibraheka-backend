using NUnit.Framework;
using FluentValidation.TestHelper;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Users.Commands.UpdateUserProfile;
using VibraHeka.Domain.Models.Results.User;

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
        UpdateUserProfileCommand command = new(new UserDTO
        {
            Id = Guid.NewGuid().ToString(),
            Email = "valid@test.com",
            FirstName = "John",
            MiddleName = "M",
            LastName = "Doe",
            Bio = "Bio",
            ProfilePictureUrl = "https://example.com/avatar.jpg",
            PhoneNumber = "+34911111222"
        });

        // When
        TestValidationResult<UpdateUserProfileCommand> result = _validator.TestValidate(command);

        // Then
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void ShouldFailWhenUserIdIsNotGuid()
    {
        // Given
        UpdateUserProfileCommand command = new(new UserDTO
        {
            Id = "not-a-guid",
            Email = "valid@test.com"
        });

        // When
        TestValidationResult<UpdateUserProfileCommand> result = _validator.TestValidate(command);

        // Then
        result.ShouldHaveValidationErrorFor(x => x.NewUserData.Id)
            .WithErrorMessage("User Id must be a valid GUID.")
            .WithErrorCode(UserErrors.InvalidUserID);
    }

    [Test]
    public void ShouldFailWhenEmailIsInvalid()
    {
        // Given
        UpdateUserProfileCommand command = new(new UserDTO
        {
            Id = Guid.NewGuid().ToString(),
            Email = "invalid-email"
        });

        // When
        TestValidationResult<UpdateUserProfileCommand> result = _validator.TestValidate(command);

        // Then
        result.ShouldHaveValidationErrorFor(x => x.NewUserData.Email)
            .WithErrorCode(UserErrors.InvalidEmail);
    }
}

