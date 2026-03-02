using System.Net.Http.Headers;
using VibraHeka.Domain.Models.Results;
using VibraHeka.Web.AcceptanceTests.Generic;

namespace VibraHeka.Web.AcceptanceTests.Users;

public abstract class GenericUserAcceptanceTest : GenericAcceptanceTest<VibraHekaProgram>
{
    protected async Task<(string UserId, string Email)> AuthenticateAsConfirmedUser()
    {
        // Given
        string email = TheFaker.Internet.Email();
        string username = TheFaker.Person.FullName;
        string userId = await RegisterAndConfirmUser(username, email, ThePassword);

        // When
        AuthenticationResult authResult = await AuthenticateUser(email, ThePassword);

        // Then
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
        return (userId, email);
    }
}

