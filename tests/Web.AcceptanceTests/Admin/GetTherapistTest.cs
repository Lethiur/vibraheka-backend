using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using NUnit.Framework;
using VibraHeka.Application.Users.Commands.AdminCreateTherapist;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Models.Results;
using VibraHeka.Domain.Models.Results.User;
using VibraHeka.Web.AcceptanceTests.Generic;

namespace VibraHeka.Web.AcceptanceTests.Admin;

[TestFixture]
public class GetTherapistTest  : GenericAcceptanceTest<VibraHekaProgram> 
{
    [Test]
    public async Task ShouldReturn403IfUserIsNotAdmin()
    {
        // Given: registered user
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmUser(TheFaker.Person.FullName, email, ThePassword);
        
        // And: Authenticated
        AuthenticationResult authenticationResult = await AuthenticateUser(email, ThePassword);
        
        // And: Header added
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);
        
        // When: The Create therapist is invoked
        HttpResponseMessage postAsJsonAsync = await Client.GetAsync("/api/v1/admin/therapists");
        
        // Then: The result should be 403
        Assert.That(postAsJsonAsync.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
    
    [Test]
    public async Task ShouldReturn403IfNotAuthenticated()
    {
        
        // When: The Create therapist is invoked
        HttpResponseMessage postAsJsonAsync = await Client.GetAsync("/api/v1/admin/therapists");
        
        // Then: The result should be 403
        Assert.That(postAsJsonAsync.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ShouldReturnTherapistsListIfLoggedInAsAdmin()
    {
        // Given: A registered admin
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Person.FullName, email, ThePassword);
        
        // And: Authenticated
        AuthenticationResult authenticationResult = await AuthenticateUser(email, ThePassword);

        // And: Header added
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);
        
        // And: Therapist created
        string therapistEmail = TheFaker.Internet.Email(); 
        await Client.PutAsJsonAsync("/api/v1/admin/addTherapist", new CreateTherapistCommand(new UserDTO(){ Email = therapistEmail, FirstName = TheFaker.Person.FullName}));

        // When: The list is requested
        HttpResponseMessage getAsync = await Client.GetAsync("/api/v1/admin/therapists");
        
        // Then: The response should be 200
        getAsync.EnsureSuccessStatusCode();
        
        // And: The content of the response should include the created therapist
        ResponseEntity entity = await getAsync.GetAsResponseEntityAndContentAs<List<UserEntity>>();
        Assert.That(entity.Content, Is.Not.Null);
        
        IEnumerable<UserEntity>? therapists = entity.GetContentAs<IEnumerable<UserEntity>>();
        IEnumerable<UserEntity> enumerable = therapists as UserEntity[] ?? therapists!.ToArray();
        Assert.That(enumerable, Is.Not.Null);
        Assert.That(enumerable, Is.Not.Empty);
        
        // And: The therapist should be in the list
        Assert.That(enumerable.Any(x => x.Email == therapistEmail));
    }
}
