using NUnit.Framework;
using FluentValidation.Results;
using VibraHeka.Application.Users.Queries.GetProfile;

namespace VibraHeka.Application.FunctionalTests.Users.Queries.GetUserProfileQueryTest;

[TestFixture]
public class GetUserProfileQueryValidatorTest
{
    private GetUserProfileQueryValidator _validator = default!;

    [SetUp]
    public void SetUp()
    {
        _validator = new GetUserProfileQueryValidator();
    }

    [Test]
    public async Task ShouldFailValidationWhenUserIdIsEmpty()
    {
        // Given
        GetUserProfileQuery query = new(string.Empty);

        // When
        ValidationResult result = await _validator.ValidateAsync(query);

        // Then
        Assert.That(result.IsValid, Is.False);
    }
}

