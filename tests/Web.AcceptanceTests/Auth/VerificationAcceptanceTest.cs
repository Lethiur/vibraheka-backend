using System.ComponentModel;
using System.Net;
using System.Net.Http.Json;
using Bogus;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Users.Commands.VerificationCode;
using VibraHeka.Domain.Entities;
using VibraHeka.Web.AcceptanceTests.Generic;

namespace VibraHeka.Web.AcceptanceTests.Auth;

[TestFixture]
public class VerificationAcceptanceTest : GenericAcceptanceTest<VibraHekaProgram>
{
    [Test]
    [DisplayName("Should verify a user")]
    public async Task ShouldVerifyAUser()
    {
        // Given: Some registered user
        Faker faker = new Faker();
        string email = faker.Internet.Email();
        string password = "Password123@";
        
        await RegisterUser( faker.Person.FullName, email, password);
        
        // And: The verification code
        VerificationCodeEntity verificationCode = await WaitForVerificationCode(email, TimeSpan.FromSeconds(10));
        
        // When: The user verifies their account
        HttpResponseMessage verificationMessage = await Client.PatchAsJsonAsync("api/v1/auth/confirm", new VerifyUserCommand(email, verificationCode.Code));
        verificationMessage.EnsureSuccessStatusCode();
        ResponseEntity responseEntity = await verificationMessage.GetAsResponseEntity();
        
        Assert.That(responseEntity.Success, Is.True, "The user should be verified successfully");
        Assert.That(responseEntity.ErrorCode, Is.Null, "The response should not contain any error code");
    }
    
    [Test]
    [DisplayName("Should fail verification with non-existent user")]
    [TestCase("user@example.com", "123456")] // Email y código válidos básicos
    public async Task ShouldFailVerificationWithNonExistentUser(string email, string code)
    {
        // Given: A valid format command but for non-existent user
        VerifyUserCommand command = new(email, code);

        // When: The client tries to verify
        HttpResponseMessage response = await Client.PatchAsJsonAsync("/api/v1/auth/confirm", command);
        ResponseEntity responseEntity = await response.GetAsResponseEntity();

        // Then: Should return appropriate error (not validation error)
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), 
            "Should return 404 Not Found when trying to verify a non-existent user");
        
        Assert.That(responseEntity.Success, Is.False, "The user should not be verified successfully");
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(UserErrors.UserNotFound));
    }
    
    [Test]
    [DisplayName("Should fail verification with wrong code")]
    public async Task ShouldFailVerificationWithWrongCode()
    {
        // Given: Some registered user
        Faker faker = new Faker();
        string email = faker.Internet.Email();
        string password = "Password123@";
        string fullName = faker.Person.FullName;

        await RegisterUser(fullName, email, password);

        // And: Wait for the verification code to be generated
        await WaitForVerificationCode(email, TimeSpan.FromSeconds(10));

        // When: The user tries to verify with wrong code
        VerifyUserCommand command = new(email, "999999"); // Wrong code
        HttpResponseMessage response = await Client.PatchAsJsonAsync("/api/v1/auth/confirm", command);
        ResponseEntity responseEntity = await response.GetAsResponseEntity();
        // Then: Should fail but not with validation error
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), 
            "Should not return BadRequest for valid format but wrong code");
        
        Assert.That(responseEntity.Success, Is.False, "The user should not be verified successfully");
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(UserErrors.WrongVerificationCode));
    }
    
    // === EMAIL TESTS ===
    [TestCase("", "123456", UserErrors.InvalidEmail)] // Email vacío
    [TestCase(null, "123456", UserErrors.InvalidEmail)] // Email null
    [TestCase("   ", "123456", UserErrors.InvalidEmail)] // Email solo espacios
    [TestCase("invalid-email", "123456", UserErrors.InvalidEmail)] // Email formato inválido
    [TestCase("@domain.com", "123456", UserErrors.InvalidEmail)] // Email sin parte local
    [TestCase("user@", "123456", UserErrors.InvalidEmail)] // Email sin dominio
    [TestCase("user.domain.com", "123456", UserErrors.InvalidEmail)] // Email sin @
    [TestCase("user@domain", "123456", UserErrors.InvalidEmail)] // Email sin TLD
    [TestCase("user..test@domain.com", "123456", UserErrors.InvalidEmail)] // Email con doble punto

    // === CODE TESTS ===
    [TestCase("test@example.com", "", UserErrors.InvalidVerificationCode)] // Code vacío
    [TestCase("test@example.com", null, UserErrors.InvalidVerificationCode)] // Code null
    [TestCase("test@example.com", "   ", UserErrors.InvalidVerificationCode)] // Code solo espacios
    [TestCase("test@example.com", "\t", UserErrors.InvalidVerificationCode)] // Code solo tab
    [TestCase("test@example.com", "\n", UserErrors.InvalidVerificationCode)] // Code solo salto de línea
    [TestCase("test@example.com", "\r\n", UserErrors.InvalidVerificationCode)] // Code CRLF
    [TestCase("test@example.com", "1", UserErrors.InvalidVerificationCode)] // Code 1 char
    [TestCase("test@example.com", "12", UserErrors.InvalidVerificationCode)] // Code 2 chars
    [TestCase("test@example.com", "123", UserErrors.InvalidVerificationCode)] // Code 3 chars
    [TestCase("test@example.com", "1234", UserErrors.InvalidVerificationCode)] // Code 4 chars
    [TestCase("test@example.com", "12345", UserErrors.InvalidVerificationCode)] // Code 5 chars (límite)
    [TestCase("test@example.com", "test", UserErrors.InvalidVerificationCode)] // Code 5 chars (límite)

    // === EDGE CASES COMBINADOS ===
    [TestCase(null, null, $"{UserErrors.InvalidEmail} | {UserErrors.InvalidVerificationCode}")] 
    [TestCase("", "", $"{UserErrors.InvalidEmail} | {UserErrors.InvalidVerificationCode}")] 
    [TestCase("   ", "   ", $"{UserErrors.InvalidEmail} | {UserErrors.InvalidVerificationCode}")] 
    [TestCase("invalid-email", "123", $"{UserErrors.InvalidEmail} | {UserErrors.InvalidVerificationCode}")] // Ambos inválidos

    [DisplayName("Should not allow verification with invalid data")]
    public async Task ShouldNotAllowVerificationWithInvalidData(string email, string code, string expectedErrorKeyword)
    {
        // Given: A verify command with invalid data
        VerifyUserCommand command = new(email, code);

        // When: The client is invoked
        HttpResponseMessage response = await Client.PatchAsJsonAsync("/api/v1/auth/confirm", command);

        // Then: Should return BadRequest
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "The status code should be BadRequest");

        // And: The response should contain the expected error message

        ResponseEntity responseObject = await response.GetAsResponseEntity();
        
        Assert.That(responseObject.Content, Is.Null, $"The response content should be null when validation fails");
        Assert.That(responseObject.ErrorCode, Is.EqualTo(expectedErrorKeyword), 
            $"The response should contain the error keyword '{expectedErrorKeyword}'. Actual error: {responseObject.ErrorCode}");
    }
    
    [Test]
    [DisplayName("Should return BadRequest when verification code is numeric but incorrect")]
    public async Task ShouldReturnBadRequestWhenVerificationCodeIsNumericButIncorrect()
    {
        // Given: A registered user and a command with a numeric but wrong code
        Faker faker = new Faker();
        string email = faker.Internet.Email();
        string password = "Password123@";
        string fullName = faker.Person.FullName;

        await RegisterUser(fullName, email, password);
        VerifyUserCommand command = new VerifyUserCommand(email, "123456");

        // When: Calling the confirm endpoint
        HttpResponseMessage response = await Client.PatchAsJsonAsync("api/v1/auth/confirm", command);

        // Then: Should return 400 BadRequest 
        // El switch en tu controlador capturará el error devuelto por el servicio
        ResponseEntity responseObject = await response.GetAsResponseEntity();
        
        Assert.That(responseObject.Content, Is.Null, $"The response content should be null when validation fails");
        Assert.That(responseObject.ErrorCode, Is.EqualTo(UserErrors.WrongVerificationCode),  
            $"The response should contain the error keyword '{UserErrors.WrongVerificationCode}'. Actual error: {responseObject.ErrorCode}");
    }
}
