using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using NUnit.Framework;
using VibraHeka.Application.Admin.Commands.CreateTherapist;
using VibraHeka.Application.Common.Models.Results;
using VibraHeka.Domain.Entities;
using VibraHeka.Web.AcceptanceTests.Generic;

namespace VibraHeka.Web.AcceptanceTests.Admin;

[TestFixture]
public class CreateTherapistTest : GenericAcceptanceTest<VibraHekaProgram>
{
    
    [Test]
    public async Task ShouldReturn403IfUserIsNotAdmin()
    {
        // Given: registered user
        string email = TheFaker.Internet.Email();
        string therapistEmail = TheFaker.Internet.Email();
        await RegisterAndConfirmUser(TheFaker.Person.FullName, email, ThePassword);
        
        // And: Authenticated
        AuthenticationResult authenticationResult = await AuthenticateUser(email, ThePassword);
        
        // And: Header added
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);
        
        // When: The Create therapist is invoked
        HttpResponseMessage postAsJsonAsync = await Client.PostAsJsonAsync("/api/v1/admin/addTherapist", new CreateTherapistCommand(therapistEmail, TheFaker.Person.FullName));
        
        // Then: The result should be 403
        Assert.That(postAsJsonAsync.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ShouldReturn403IfNotAuthenticated()
    {
        
        // When: The Create therapist is invoked
        HttpResponseMessage postAsJsonAsync = await Client.PostAsJsonAsync("/api/v1/admin/addTherapist", new CreateTherapistCommand(TheFaker.Internet.Email(), TheFaker.Person.FullName));
        
        // Then: The result should be 403
        Assert.That(postAsJsonAsync.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ShouldAddTherapistIfLoggedInAsAdmin()
    {
        // Given: A registered user
        string email = TheFaker.Internet.Email();
        string userID = await RegisterAndConfirmUser(TheFaker.Person.FullName, email, ThePassword);
        
        // And: Admin created
        await PromoteToAdmin(TheFaker.Person.FullName, email, userID);
        
        // And: Authenticated
        AuthenticationResult authenticationResult = await AuthenticateUser(email, ThePassword);
        
        // And: Header added
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);
        
        // When: The Create therapist is invoked
        HttpResponseMessage postAsJsonAsync = await Client.PostAsJsonAsync("/api/v1/admin/addTherapist", new CreateTherapistCommand(TheFaker.Internet.Email(), TheFaker.Person.FullName));
        
        // Then: The result should be 200
        Assert.That(postAsJsonAsync.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        // And: The therapist should be added
        ResponseEntity entity = await postAsJsonAsync.GetAsResponseEntity();
        Assert.That(entity.Success, Is.True);
    }
    
}
