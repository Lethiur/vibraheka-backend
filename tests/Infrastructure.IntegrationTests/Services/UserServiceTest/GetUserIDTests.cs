using System.ComponentModel;
using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Exceptions;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.UserServiceTest;

[TestFixture]
public class GetUserIDTests : GenericCognitoServiceTest
{
    [Test]
    [DisplayName("Should return user id when user exists")]
    public async Task ShouldReturnUserIdWhenUserExists()
    {
        // Given: A registered user
        string email = GenerateUniqueEmail("test-get-user-id");
        string registeredUserId = await RegisterUser(email);

        // When: Getting the user id by email
        Result<string> result = await _userService.GetUserID(email, CancellationToken.None);

        // Then: It should return the same Cognito sub
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null.Or.Empty);
        Assert.That(result.Value, Is.EqualTo(registeredUserId));
    }

    [Test]
    [DisplayName("Should fail with UserNotFound when user does not exist")]
    public async Task ShouldFailWithUserNotFoundWhenUserDoesNotExist()
    {
        // Given: A non-existent email
        string email = $"ghost-{Guid.NewGuid()}@example.com";

        // When: Getting the user id by email
        Result<string> result = await _userService.GetUserID(email, CancellationToken.None);

        // Then: It should map to our domain error
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.UserNotFound));
    }

    [TestCase("", TestName = "Empty email")]
    [TestCase("   ", TestName = "Whitespace email")]
    [TestCase(null, TestName = "Null email")]
    [DisplayName("Should fail with InvalidForm when email is invalid")]
    public async Task ShouldFailWithInvalidFormWhenEmailIsInvalid(string? email)
    {
        Result<string> result = await _userService.GetUserID(email!, CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.AnyOf(UserErrors.InvalidForm, UserErrors.UnexpectedError));
    }
}
