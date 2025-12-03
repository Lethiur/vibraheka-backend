using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Net;
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
        Faker faker = new();
        RegisterUserCommand command = new(faker.Internet.Email(), "Password123@", "Hola policia");
        
        // When: The client is invoked
        HttpResponseMessage postAsJsonAsync = await Client.PostAsJsonAsync("/api/v1/auth/register", command);

        Assert.That(postAsJsonAsync.StatusCode, Is.EqualTo(HttpStatusCode.OK), "The status code should be OK");
    }
    
    [Test]
    [DisplayName("The response should not be allowed if the data is wrong")]
    public async Task ShouldNotAllowRegistrationNWithWrongData()
    {
        Faker faker = new();
        RegisterUserCommand command = new(faker.Internet.Email(), "p", "Hola policia");
        
        // When: The client is invoked
        HttpResponseMessage postAsJsonAsync = await Client.PostAsJsonAsync("/api/v1/auth/register", command);

        Assert.That(postAsJsonAsync.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "The status code should be OK");
    }
}
