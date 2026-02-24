using System.Net.Http.Headers;
using VibraHeka.Domain.Models.Results;
using VibraHeka.Web.AcceptanceTests.Generic;

namespace VibraHeka.Web.AcceptanceTests.Subscription;

public abstract class GenericSubscriptionAcceptanceTest : GenericAcceptanceTest<VibraHekaProgram>
{
    protected async Task<AuthenticationResult> AuthenticateAsConfirmedUser()
    {
        // Given
        string email = TheFaker.Internet.Email();
        string username = TheFaker.Person.FullName;
        await RegisterAndConfirmUser(username, email, ThePassword);

        // When
        AuthenticationResult authResult = await AuthenticateUser(email, ThePassword);

        // Then
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
        return authResult;
    }
}
