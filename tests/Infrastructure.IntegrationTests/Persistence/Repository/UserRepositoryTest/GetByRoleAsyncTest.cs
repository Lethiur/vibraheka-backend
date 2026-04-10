using System.ComponentModel;
using CSharpFunctionalExtensions;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Entities;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.UserRepositoryTest;

[TestFixture]
public class GetByRoleAsyncTest : GenericUserRepositoryTest
{
    [Test]
    [DisplayName("Should return a list of users when users with the specified role exist")]
    public async Task ShouldReturnListOfUsersWhenUsersWithRoleExist()
    {
        // Given: usuarios persistidos con el mismo rol.
        UserRole role = UserRole.Therapist;
        UserProfileEntity user1 = CreateValidUser();
        user1.Role = role;
        UserProfileEntity user2 = CreateValidUser();
        user2.Role = role;

        await _userRepository.AddAsync(user1);
        await _userRepository.AddAsync(user2);

        // When: se consultan usuarios por rol.
        Result<IEnumerable<UserProfileEntity>> result = await _userRepository.GetByRoleAsync(role);

        // Then: debe devolverse una lista que incluya ambos usuarios.
        Assert.That(result.IsSuccess, Is.True);
        List<UserProfileEntity> users = result.Value.ToList();
        Assert.That(users.Any(u => u.Id == user1.Id), Is.True);
        Assert.That(users.Any(u => u.Id == user2.Id), Is.True);
        Assert.That(users.All(u => u.Role == role), Is.True);

        await CleanupUser(user1.Id);
        await CleanupUser(user2.Id);
    }

    [Test]
    [DisplayName("Should correctly map all domain properties when retrieving by role")]
    public async Task ShouldCorrectlyMapAllPropertiesWhenRetrievingByRole()
    {
        // Given: un usuario con todos los datos principales.
        UserProfileEntity userProfileEntity = CreateValidUser();
        userProfileEntity.Role = UserRole.Therapist;
        await _userRepository.AddAsync(userProfileEntity);

        // When: se consulta por rol.
        Result<IEnumerable<UserProfileEntity>> result = await _userRepository.GetByRoleAsync(UserRole.Therapist);

        // Then: las propiedades mapeadas deben coincidir.
        UserProfileEntity retrievedUserProfileEntity = result.Value.First(u => u.Id == userProfileEntity.Id);
        Assert.That(retrievedUserProfileEntity.FirstName, Is.EqualTo(userProfileEntity.FirstName));
        Assert.That(retrievedUserProfileEntity.Email, Is.EqualTo(userProfileEntity.Email));
        Assert.That(retrievedUserProfileEntity.Role, Is.EqualTo(UserRole.Therapist));

        await CleanupUser(userProfileEntity.Id);
    }

    [Test]
    [DisplayName("Should return failure when querying a non-existing table")]
    public async Task ShouldReturnFailureWhenQueryingNonExistingTable()
    {
        // Given: un repositorio apuntando a una tabla inexistente.
        AWSConfig invalidConfig = new()
        {
            EmailTemplatesBucketName = _configuration.EmailTemplatesBucketName,
            UserCodesTable = _configuration.UserCodesTable,
            EmailTemplatesTable = _configuration.EmailTemplatesTable,
            UsersTable = $"non-existing-users-table-{Guid.NewGuid():N}",
#if DEBUG
            CodesTable = _configuration.CodesTable,
#endif
            ClientId = _configuration.ClientId,
            UserPoolId = _configuration.UserPoolId,
            Location = _configuration.Location,
            Profile = _configuration.Profile,
            PasswordResetTokenSecret = _configuration.PasswordResetTokenSecret,
            ActionLogTable = _configuration.ActionLogTable,
            SubscriptionTable = _configuration.SubscriptionTable,
            SubscriptionUserIdIndex = _configuration.SubscriptionUserIdIndex,
            SettingsNameSpace = _configuration.SettingsNameSpace
        };
        UserRepository invalidRepository = new(_dynamoContext, invalidConfig);

        // When: se consulta por rol contra la tabla inexistente.
        Result<IEnumerable<UserProfileEntity>> result = await invalidRepository.GetByRoleAsync(UserRole.Therapist);

        // Then: debe devolverse failure con mensaje de query.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Does.Contain("Error querying users by role"));
    }
}
