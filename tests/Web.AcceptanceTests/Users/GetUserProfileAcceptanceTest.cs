using System.IdentityModel.Tokens.Jwt;
using System.Net;
using CSharpFunctionalExtensions;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;
using VibraHeka.Web.AcceptanceTests.Utils;
using VibraHeka.Web.Authentication;
using VibraHeka.Web.Users;
using BadRequestResponse = VibraHeka.Web.Users.BadRequestResponse;

namespace VibraHeka.Web.AcceptanceTests.Users;

[TestFixture]
public class GetUserProfileAcceptanceTest : GenericUserAcceptanceTest
{
    [Test]
    public async Task ShouldReturnUnauthorizedWhenRequestIsNotAuthenticated()
    {
        // Given: no authenticated user context.
        Client.DefaultRequestHeaders.Remove("Authorization");
        string userId = Guid.NewGuid().ToString();

        // When: requesting profile for any user id.
        HttpResponseMessage response = await Client.GetAsync($"/api/v1/users/{userId}");

        // Then: endpoint should return unauthorized.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ShouldReturnUserProfileWhenAuthenticated()
    {
        // Given: a confirmed and authenticated user.
        AuthenticateUserResponse authenticateAsConfirmedUser = await AuthenticateAsConfirmedUser();
        
        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(authenticateAsConfirmedUser.AccessToken);

        // When: requesting own profile.
        HttpResponseMessage response = await Client.GetAsync($"/api/v1/users/{jwtSecurityToken.Subject}");

        // Then: profile payload should be returned successfully.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        UserDTO entity = await response.ParseContentAsync<UserDTO>();

        Assert.That(entity, Is.Not.Null);
        Assert.That(entity.Id, Is.EqualTo(jwtSecurityToken.Subject));
    }

    [Test]
    public async Task ShouldReturnBadRequestAndUserNotFoundWhenUserDoesNotExist()
    {
        // Given: an authenticated requester and a random non-existing target user id.
        _ = await AuthenticateAsConfirmedUser();
        string unknownUserId = Guid.NewGuid().ToString();

        // When: requesting profile for a non-existing user.
        HttpResponseMessage response = await Client.GetAsync($"/api/v1/users/{unknownUserId}");

        // Then: service should map the miss to repository user-not-found error.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        BadRequestResponse entity = await response.ParseContentAsync<BadRequestResponse>();
        Assert.That(entity.ErrorCode, Is.EqualTo(UserErrors.UserNotFound));
    }

    [Test]
    public async Task ShouldNotExposePhoneNumberWhenRequestingAnotherUsersProfile()
    {
        // Given: two confirmed users and authentication as the first user.
        _ = await AuthenticateAsConfirmedUser();
        string targetEmail = TheFaker.Internet.Email();
        string targetId = await RegisterAndConfirmUser(TheFaker.Person.FullName, targetEmail, ThePassword);

        IUserRepository userRepository = GetObjectFromFactory<IUserRepository>();
        Result<UserEntity> targetResult = await userRepository.GetByIdAsync(targetId, CancellationToken.None);
        Assert.That(targetResult.IsSuccess, Is.True);

        UserEntity targetUser = targetResult.Value;
        targetUser.PhoneNumber = "+34911111222";
        await userRepository.AddAsync(targetUser);

        // When: requesting profile of another existing user.
        HttpResponseMessage response = await Client.GetAsync($"/api/v1/users/{targetId}");

        // Then: endpoint returns profile but hides phone number for non-owner.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        UserDTO entity = await response.ParseContentAsync<UserDTO>();
        
        Assert.That(entity, Is.Not.Null);
        Assert.That(entity.Id, Is.EqualTo(targetId));
        Assert.That(entity.PhoneNumber, Is.Null.Or.Empty);
    }
}
