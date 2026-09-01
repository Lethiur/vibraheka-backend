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
        RemoveAuthHeader();

        // When: requesting profile for any user id.
        await PerformCallAndExpectStatusCode(() => InvokeGetUserProfileEndpoint(Guid.NewGuid()), HttpStatusCode.Unauthorized);
        
    }

    [Test]
    public async Task ShouldReturnUserProfileWhenAuthenticated()
    {
        // Given: a confirmed and authenticated user.
        await AuthenticateAsNewUser();
        
        // And: The user ID
        Guid userId = GetuserID();

        // When: requesting own profile.
        UserDTO entity = await PerformGetUserProfile(userId);

        // Then: profile payload should be returned successfully.
        Assert.That(entity, Is.Not.Null);
        Assert.That(entity.Id, Is.EqualTo(userId));
    }

    [Test]
    public async Task ShouldReturnBadRequestAndUserNotFoundWhenUserDoesNotExist()
    {
        // Given: an authenticated requester and a random non-existing target user id.
        await AuthenticateAsNewUser();

        // When: requesting a profile for a non-existing user.
        // Then: service should map the missing to repository user-not-found error.
        await PerformCallAndExpectError(() => InvokeGetUserProfileEndpoint(Guid.NewGuid()),UserErrors.UserNotFound);
    }

    [Test]
    public async Task ShouldNotExposePhoneNumberWhenRequestingAnotherUsersProfile()
    {
        // Given: two confirmed users and authentication as the first user.
        await AuthenticateAsNewUser();
        string targetEmail = TheFaker.Internet.Email();
        string targetId = await RegisterAndConfirmUser(targetEmail);

        
        IUserRepository userRepository = GetObjectFromFactory<IUserRepository>();
        (bool isSuccess, _, UserEntity targetUser) = await userRepository.GetByIdAsync(targetId, CancellationToken.None);
        Assert.That(isSuccess, Is.True);
        targetUser.PhoneNumber = "+34911111222";
        await userRepository.AddAsync(targetUser);

        // When: requesting profile of another existing user.
        UserDTO entity = await PerformGetUserProfile(Guid.Parse(targetId));


        // Then: endpoint returns profile but hides phone number for non-owner.
        Assert.That(entity, Is.Not.Null);
        Assert.That(entity.Id, Is.EqualTo(targetId));
        Assert.That(entity.PhoneNumber, Is.Null.Or.Empty);
    }
}
