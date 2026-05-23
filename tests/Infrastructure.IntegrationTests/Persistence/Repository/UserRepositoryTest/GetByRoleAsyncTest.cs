using System.ComponentModel;
using CSharpFunctionalExtensions;
using VibraHeka.Domain.Entities;

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
        UserEntity user1 = CreateValidUser();
        user1.Role = role;
        UserEntity user2 = CreateValidUser();
        user2.Role = role;

        await _userRepository.AddAsync(user1);
        await _userRepository.AddAsync(user2);

        // When: se consultan usuarios por rol.
        Result<List<UserEntity>> result = await _userRepository.GetByRoleAsync(role);

        // Then: debe devolverse una lista que incluya ambos usuarios.
        Assert.That(result.IsSuccess, Is.True);
        List<UserEntity> users = result.Value.ToList();
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
        UserEntity userEntity = CreateValidUser();
        userEntity.Role = UserRole.Therapist;
        await _userRepository.AddAsync(userEntity);

        // When: se consulta por rol.
        Result<List<UserEntity>> result = await _userRepository.GetByRoleAsync(UserRole.Therapist);

        // Then: las propiedades mapeadas deben coincidir.
        UserEntity retrievedUserEntity = result.Value.First(u => u.Id == userEntity.Id);
        Assert.That(retrievedUserEntity.FirstName, Is.EqualTo(userEntity.FirstName));
        Assert.That(retrievedUserEntity.Email, Is.EqualTo(userEntity.Email));
        Assert.That(retrievedUserEntity.Role, Is.EqualTo(UserRole.Therapist));

        await CleanupUser(userEntity.Id);
    }
}
