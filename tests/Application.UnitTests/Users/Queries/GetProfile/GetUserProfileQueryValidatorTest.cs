using NUnit.Framework;
using FluentValidation.TestHelper;
using VibraHeka.Application.Users.Queries.GetProfile;
using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Application.UnitTests.Users.Queries.GetProfile;

[TestFixture]
public class GetUserProfileQueryValidatorTest
{
    private GetUserProfileQueryValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new GetUserProfileQueryValidator();
    }

    [Test]
    public void ShouldPassValidationWhenUserIdIsValid()
    {
        // Given
        GetUserProfileQuery query = new(Guid.NewGuid().ToString());

        // When
        TestValidationResult<GetUserProfileQuery> result = _validator.TestValidate(query);

        // Then
        result.ShouldNotHaveAnyValidationErrors();
    }

    [TestCase(null)]
    [TestCase("")]
    public void ShouldFailValidationWhenUserIdIsMissing(string? invalidUserId)
    {
        // Given
        GetUserProfileQuery query = new(invalidUserId!);

        // When
        TestValidationResult<GetUserProfileQuery> result = _validator.TestValidate(query);

        // Then
        result.ShouldHaveValidationErrorFor(x => x.UserID)
            .WithErrorMessage(ProfileErrors.InvalidProfileID);
    }
}

