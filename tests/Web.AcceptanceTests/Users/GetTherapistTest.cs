using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using NUnit.Framework;
using VibraHeka.Domain.Entities;
using VibraHeka.Web.AcceptanceTests.Generic;
using VibraHeka.Web.AcceptanceTests.Utils;
using VibraHeka.Web.Authentication;
using VibraHeka.Web.Users;

namespace VibraHeka.Web.AcceptanceTests.Users;

[TestFixture]
public class GetTherapistTest : GenericUserAcceptanceTest
{
    [Test]
    public async Task ShouldReturn403IfUserIsNotAdmin()
    {
        // Given: Registered user
        await AuthenticateAsNewUser();

        // When: Calling Get Therapists endpoint
        await PerformCallAndExpectStatusCode(InvokeGetTherapistsEndpoint, HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task ShouldReturn401IfNotAuthenticated()
    {
        // Given: No authentication token
        RemoveAuthHeader();
        
        // When: Calling Get Therapists endpoint
        await PerformCallAndExpectStatusCode(InvokeGetTherapistsEndpoint, HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task ShouldReturnTherapistsListIfLoggedInAsAdmin()
    {
        // Given: Registered admin user
        await AuthenticateAsNewAdmin();

        // And: A created therapist
        CreateTherapistResponse createTherapistResponse = await PerformCreateTherapist(ValidCreateTherapistRequest());
        
        // When: Retrieving the therapist list
        IEnumerable<UserDTO> enumerable = await PerformGetTherapists();

        // Then: The list should contain the created therapist
        IEnumerable<UserDTO> userDtos = enumerable.ToList();
        Assert.That(userDtos, Is.Not.Null);
        Assert.That(userDtos, Is.Not.Empty);
        Assert.That(userDtos.Any(x => x.Id == createTherapistResponse.Id));
    }
}
