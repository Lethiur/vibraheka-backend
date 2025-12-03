using System.ComponentModel;
using System.Net.Http.Json;
using Bogus;
using NUnit.Framework;
using VibraHeka.Application.Users.Commands;
using VibraHeka.Web.AcceptanceTests.Generic;

namespace VibraHeka.Web.AcceptanceTests.Auth;

public class RegisterAcceptanceTest : GenericAcceptanceTest<VibraHekaProgram>
{
    [Test]
    [DisplayName("Should register a new user")]
    public async Task ShouldRegisterANewUser()
    {
        // Given: A command
        Faker faker = new Faker();
        RegisterUserCommand command = new(faker.Internet.Email(), "Password123@", "Hola policia");
        
        // When: The client is invoked
        HttpResponseMessage postAsJsonAsync = await Client.PostAsJsonAsync("/api/v1/auth/register", command);
        
        Console.WriteLine(postAsJsonAsync.StatusCode);
    }
}
