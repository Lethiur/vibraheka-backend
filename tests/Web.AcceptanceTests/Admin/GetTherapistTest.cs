using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using NUnit.Framework;
using VibraHeka.Domain.Entities;
using VibraHeka.Web.AcceptanceTests.Generic;
using VibraHeka.Web.AcceptanceTests.Utils;
using VibraHeka.Web.Authentication;
using VibraHeka.Web.Users;

namespace VibraHeka.Web.AcceptanceTests.Admin;

[TestFixture]
public class GetTherapistTest : GenericAcceptanceTest<VibraHekaProgram>
{
    [Test]
    public async Task ShouldReturn403IfUserIsNotAdmin()
    {
        // Given: Registered user
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmUser(TheFaker.Person.FullName, email, ThePassword);

        // And: Authenticated as non-admin
        AuthenticateUserResponse authenticationResult = await AuthenticateUser(email, ThePassword);

        // And: Authorization header with user token
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);

        // When: Calling Get Therapists endpoint
        HttpResponseMessage postAsJsonAsync = await Client.GetAsync("/api/v1/admin/therapists");

        // Then: Request is unauthorized
        Assert.That(postAsJsonAsync.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ShouldReturn403IfNotAuthenticated()
    {
        // Given: No authentication token

        // When: Calling Get Therapists endpoint
        HttpResponseMessage postAsJsonAsync = await Client.GetAsync("/api/v1/admin/therapists");

        // Then: Request is unauthorized
        Assert.That(postAsJsonAsync.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ShouldReturnTherapistsListIfLoggedInAsAdmin()
    {
        // Given: Registered admin user
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Person.FullName, email, ThePassword);

        // And: Authenticated as admin
        AuthenticateUserResponse authenticationResult = await AuthenticateUser(email, ThePassword);

        // And: Authorization header with admin token
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);

        // And: A therapist created with a valid DTO payload
        string therapistEmail = $"{Guid.NewGuid():N}@example.com";
        await Client.PutAsJsonAsync("/api/v1/admin/addTherapist", new CreateTherapistRequest()
        {
            Email = therapistEmail,
            FirstName = "Valid Therapist",
            MiddleName = "Valid Middle",
            LastName = "Valid Last",
            PhoneNumber = "+34911111222",
            TimezoneID = "Europe/Madrid"
        });

        // When: Requesting therapists list
        HttpResponseMessage getAsync = await Client.GetAsync("/api/v1/users/admin/therapists");

        // Then: Response is OK
        getAsync.EnsureSuccessStatusCode();

        // And: Response contains the created therapist
        List<UserEntity> enumerable = await getAsync.ParseContentAsync<List<UserEntity>>();
        
        Assert.That(enumerable, Is.Not.Null);
        Assert.That(enumerable, Is.Not.Empty);
        Assert.That(enumerable.Any(x => x.Email == therapistEmail));
    }
}
