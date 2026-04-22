using System.ComponentModel;
using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Exceptions;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.UserServiceTest;

[TestFixture]
public class RegisterUserAsync : GenericCognitoServiceTest
{

    [Test]
    [DisplayName("Should successfully register user with valid data")]
    public async Task ShouldRegisterUserSuccessfullyWhenValidDataProvided()
    {
        // Given: Valid user data
        string email = _faker.Internet.Email();
        string password = "ValidPassword123!";
        string fullName = _faker.Person.FullName;

        // When: Registering the user
        Result<string> result = await UserService.RegisterUserAsync(email, password, fullName);

        // Then: Should return success with UserSub
        Assert.That(result.IsSuccess, Is.True,
            $"Expected registration to succeed but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");
        Assert.That(result.Value, Is.Not.Null,
            "Expected a non-null UserSub in the result value");
        Assert.That(result.Value, Is.Not.Empty,
            "Expected a non-empty UserSub in the result value");
    }

    [Test]
    [DisplayName("Should successfully register user with complex email")]
    public async Task ShouldRegisterUserSuccessfullyWhenComplexEmailProvided()
    {
        // Given: Complex but valid email
        string email = $"test.user+integration${_faker.Internet.Email()}";
        string password = "ValidPassword123!";
        string fullName = "John Doe";

        // When: Registering the user
        Result<string> result = await UserService.RegisterUserAsync(email, password, fullName);

        // Then: Should return success
        Assert.That(result.IsSuccess, Is.True,
            $"Expected registration to succeed but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");
        Assert.That(result.Value, Is.Not.Null,
            "Expected a non-null UserSub in the result value");
    }

    [Test]
    [DisplayName("Should successfully register user with special characters in name")]
    public async Task ShouldRegisterUserSuccessfullyWhenSpecialCharactersInName()
    {
        // Given: Name with special characters
        string email = _faker.Internet.Email();
        string password = "ValidPassword123!";
        string fullName = "José María O'Connor-Smith";

        // When: Registering the user
        Result<string> result = await UserService.RegisterUserAsync(email, password, fullName);

        // Then: Should return success
        Assert.That(result.IsSuccess, Is.True,
            $"Expected registration to succeed but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");
        Assert.That(result.Value, Is.Not.Null,
            "Expected a non-null UserSub in the result value");
    }

    #region RegisterUserAsync - Failure Cases

    [Test]
    [DisplayName("Should fail when user already exists")]
    public async Task ShouldReturnUserAlreadyExistWhenUserAlreadyExists()
    {
        // Given: A user that already exists
        string email = _faker.Internet.Email();
        await RegisterUser(email);

        // When: Trying to register the same user again
        Result<string> secondResult = await UserService.RegisterUserAsync(email, "Password123@", "Hello policeman");

        // Then: Should fail with UserAlreadyExist error
        Assert.That(secondResult.IsFailure, Is.True,
            "Expected second registration to fail but got success");
        Assert.That(secondResult.Error, Is.EqualTo(UserErrors.UserAlreadyExist),
            $"Expected error '{UserErrors.UserAlreadyExist}' but got: '{secondResult.Error}'");
    }

    [TestCase("", "ValidPassword123!", "John Doe", TestName = "Empty email")]
    [TestCase("   ", "ValidPassword123!", "John Doe", TestName = "Whitespace email")]
    [TestCase(null, "ValidPassword123!", "John Doe", TestName = "Null email")]
    [TestCase("test@example.com", "", "John Doe", TestName = "Empty password")]
    [TestCase("test@example.com", "   ", "John Doe", TestName = "Whitespace password")]
    [TestCase("test@example.com", null, "John Doe", TestName = "Null password")]
    [TestCase("test@example.com", "ValidPassword123!", "", TestName = "Empty full name")]
    [TestCase("test@example.com", "ValidPassword123!", null, TestName = "Null full name")]
    [DisplayName("Should fail when required fields are empty or null")]
    public async Task ShouldReturnInvalidFormWhenRequiredFieldsAreEmptyOrNull(string? email, string? password, string? fullName)
    {
        // When: Trying to register with empty/null fields
        Result<string> result = await UserService.RegisterUserAsync(email!, password!, fullName!);

        // Then: Should fail with InvalidForm error (AWS treats empty fields as invalid parameters)
        Assert.That(result.IsFailure, Is.True,
            "Expected registration to fail with invalid form fields but got success");
        Assert.That(result.Error, Is.EqualTo(UserErrors.InvalidForm),
            $"Expected error '{UserErrors.InvalidForm}' but got: '{result.Error}'");
    }


    [TestCase("123", TestName = "Too short password")]
    [TestCase("password", TestName = "No uppercase")]
    [TestCase("PASSWORD", TestName = "No lowercase")]
    [TestCase("Password", TestName = "No numbers")]
    [TestCase("Password123", TestName = "No special characters")]
    [DisplayName("Should fail when password is invalid")]
    public async Task ShouldReturnInvalidPasswordWhenPasswordIsInvalid(string invalidPassword)
    {
        // Given: Invalid password
        string email = _faker.Internet.Email();
        string fullName = "John Doe";

        // When: Trying to register with invalid password
        Result<string> result = await UserService.RegisterUserAsync(email, invalidPassword, fullName);

        // Then: Should fail with InvalidPassword error
        Assert.That(result.IsFailure, Is.True,
            $"Expected registration to fail for password '{invalidPassword}' but got success");
        Assert.That(result.Error, Is.EqualTo(UserErrors.InvalidPassword),
            $"Expected error '{UserErrors.InvalidPassword}' but got: '{result.Error}'");
    }

    [TestCase("", "John Doe", TestName = "Empty email")]
    [TestCase("invalid-email", "John Doe", TestName = "Invalid email format")]
    [TestCase("valid@email.com", "", TestName = "Empty full name")]
    [TestCase("valid@email.com", null, TestName = "Null full name")]
    [DisplayName("Should fail when required parameters are invalid")]
    public async Task ShouldReturnInvalidFormWhenRequiredParametersAreInvalid(string email, string? fullName)
    {
        // Given: Invalid parameters
        string password = "ValidPassword123!";

        // When: Trying to register with invalid parameters
        Result<string> result = await UserService.RegisterUserAsync(email, password, fullName!);

        // Then: Should fail with InvalidForm error
        Assert.That(result.IsFailure, Is.True,
            $"Expected registration to fail for email='{email}', fullName='{fullName}' but got success");
        Assert.That(result.Error, Is.EqualTo(UserErrors.InvalidForm),
            $"Expected error '{UserErrors.InvalidForm}' but got: '{result.Error}'");
    }

    #endregion

    #region Edge Cases

    [Test]
    [DisplayName("Should handle very long full name")]
    public async Task ShouldHandleLongFullNameGracefully()
    {
        // Given: Very long full name (testing limits)
        string email = GenerateUniqueEmail();
        string password = "ValidPassword123!";
        string fullName = new('A', 200); // Very long name

        // When: Registering with long name
        Result<string> result = await UserService.RegisterUserAsync(email, password, fullName);

        // Then: Should either succeed or fail gracefully (no unhandled exception)
        Assert.That(
            result.IsSuccess || result.IsFailure,
            Is.True,
            "Expected registration with a very long name to return a valid Result (success or failure) without throwing");

        if (result.IsSuccess)
        {
            Assert.That(result.Value, Is.Not.Null.And.Not.Empty,
                "Expected a non-empty UserSub when registration succeeds for long name");
        }
        else
        {
            Assert.That(result.Error, Is.Not.Null.And.Not.Empty,
                "Expected a non-empty error code when registration fails for long name");
        }
    }

    [Test]
    [DisplayName("Should handle concurrent registrations")]
    public async Task ShouldHandleConcurrentRegistrationsSuccessfully()
    {
        // Given: Multiple different users
        List<Task<Result<string>>> tasks = new();

        for (int i = 0; i < 5; i++)
        {
            string email = $"concurrent{i}{_faker.Internet.Email()}";

            tasks.Add(UserService.RegisterUserAsync(
                email,
                "ValidPassword123!",
                $"User {i}"));
        }

        // When: Running concurrent registrations
        Result<string>[] results = await Task.WhenAll(tasks);

        // Then: All should succeed
        foreach (Result<string> result in results)
        {
            Assert.That(result.IsSuccess, Is.True,
                $"Expected concurrent registration to succeed but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");
            Assert.That(result.Value, Is.Not.Null.And.Not.Empty,
                "Expected a non-empty UserSub for each concurrent registration result");
        }
    }

    #endregion
}
