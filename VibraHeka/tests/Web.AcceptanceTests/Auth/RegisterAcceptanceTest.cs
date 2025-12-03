using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Net;
using System.Net.Http.Json;
using Bogus;
using Newtonsoft.Json;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Users.Commands;
using VibraHeka.Domain.Entities;
using VibraHeka.Web.AcceptanceTests.Generic;

namespace VibraHeka.Web.AcceptanceTests.Auth;

public class RegisterAcceptanceTest : GenericAcceptanceTest<VibraHekaProgram>
{
    [Test]
    [DisplayName("Should register a new user")]
    public async Task ShouldRegisterANewUser()
    {
        // Given: A command
        Faker faker = new();
        RegisterUserCommand command = new(faker.Internet.Email(), "Password123@", "Hola policia");
        
        // When: The client is invoked
        HttpResponseMessage postAsJsonAsync = await Client.PostAsJsonAsync("/api/v1/auth/register", command);

        Assert.That(postAsJsonAsync.StatusCode, Is.EqualTo(HttpStatusCode.OK), "The status code should be OK");
    }
    
   // === EMAIL TESTS ===
    [TestCase("", "Password123@", "John Doe", UserException.InvalidEmail)] // Email vacío
    [TestCase(null, "Password123@", "John Doe", UserException.InvalidEmail)] // Email null
    [TestCase("   ", "Password123@", "John Doe", UserException.InvalidEmail)] // Email solo espacios
    [TestCase("invalid-email", "Password123@", "John Doe", UserException.InvalidEmail)] // Email formato inválido
    [TestCase("@domain.com", "Password123@", "John Doe", UserException.InvalidEmail)] // Email sin parte local
    [TestCase("user@", "Password123@", "John Doe", UserException.InvalidEmail)] // Email sin dominio
    [TestCase("user.domain.com", "Password123@", "John Doe", UserException.InvalidEmail)] // Email sin @
    
    // === PASSWORD TESTS ===
    [TestCase("test@example.com", "", "John Doe", UserException.InvalidPassword)] // Password vacío
    [TestCase("test@example.com", null, "John Doe", UserException.InvalidPassword)] // Password null
    [TestCase("test@example.com", "   ", "John Doe", UserException.InvalidPassword)] // Password solo espacios
    [TestCase("test@example.com", "1", "John Doe", UserException.InvalidPassword)] // Password 1 char
    [TestCase("test@example.com", "12", "John Doe", UserException.InvalidPassword)] // Password 2 chars
    [TestCase("test@example.com", "123", "John Doe", UserException.InvalidPassword)] // Password 3 chars
    [TestCase("test@example.com", "1234", "John Doe", UserException.InvalidPassword)] // Password 4 chars
    [TestCase("test@example.com", "12345", "John Doe", UserException.InvalidPassword)] // Password 5 chars (límite)
    
    // === FULLNAME TESTS ===
    [TestCase("test@example.com", "Password123@", "", UserException.InvalidFullName)] // FullName vacío
    [TestCase("test@example.com", "Password123@", null, UserException.InvalidFullName)] // FullName null
    [TestCase("test@example.com", "Password123@", "   ", UserException.InvalidFullName)] // FullName solo espacios
    [TestCase("test@example.com", "Password123@", "\t", UserException.InvalidFullName)] // FullName solo tab
    [TestCase("test@example.com", "Password123@", "\n", UserException.InvalidFullName)] // FullName solo salto de línea
    [TestCase("test@example.com", "Password123@", "\r\n", UserException.InvalidFullName)] // FullName CRLF
    [TestCase("test@example.com", "Password123@", "A", UserException.InvalidFullName)] // FullName 1 char
    [TestCase("test@example.com", "Password123@", "AB", UserException.InvalidFullName)] // FullName 2 chars (límite)
    [TestCase("test@example.com", "Password123@", "  A  ", UserException.InvalidFullName)] // FullName con espacios al inicio/final
    
    // === EDGE CASES COMBINADOS ===
    [TestCase(null, null, null, "E-006 | E-001 | E-007")] // Todo null (primera validación que falle)
    [TestCase("", "", "", "E-006 | E-001 | E-007")] // Todo vacío (primera validación que falle)
    [TestCase("   ", "   ", "   ", "E-006 | E-001 | E-007")] // Todo espacios (primera validación que falle)
    
    [DisplayName("Should not allow registration with wrong data")]
    public async Task ShouldNotAllowRegistrationWithWrongData(string email, string password, string fullName, string expectedErrorKeyword)
    {
        // Given: A command with invalid data
        RegisterUserCommand command = new(email, password, fullName);
    
        // When: The client is invoked
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/v1/auth/register", command);

        // Then: Should return BadRequest
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "The status code should be BadRequest");
        
        // And: The response should contain the expected error message
        var responseContent = await response.Content.ReadAsStringAsync();

        ResponseEntity responseObject = JsonConvert.DeserializeObject<ResponseEntity>(responseContent) ?? throw new DataException("The response content could not be deserialized to a ResponseEntity object.");
        Assert.That(responseObject.Content, Is.Null,$"The response should contain the error keyword '{expectedErrorKeyword}'. Actual response: {responseContent}");
        Assert.That(responseObject.ErrorCode, Is.EqualTo(expectedErrorKeyword));
    }
}
