using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Mappers;
using VibraHeka.Infrastructure.Persistence.Repository;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.SubscriptionRepositoryTest;

[TestFixture]
public class GetSubscriptionDetailsForUserTest : GenericSubscriptionRepositoryIntegrationTest
{
    [Test]
    public async Task ShouldReturnSubscriptionForUserWhenItExists()
    {
        // Given: un registro de suscripcion existente para el usuario.
        string userId = Guid.NewGuid().ToString();
        SubscriptionEntity entity = new()
        {
            SubscriptionID = Guid.NewGuid().ToString(),
            UserID = userId,
            ExternalSubscriptionItemID = _stripeConfig.SubscriptionID,
            ExternalCustomerID = "cus_test_" + Guid.NewGuid().ToString("N"),
            SubscriptionStatus = SubscriptionStatus.Created,
            Status = OrderStatus.Pending,
            Created = DateTime.UtcNow,
            CreatedBy = "integration-test"
        };

        await _repository.SaveSubscriptionAsync(entity, CancellationToken.None);

        // When: se consulta la suscripcion por el user id.
        Result<SubscriptionEntity> result = await _repository.GetSubscriptionDetailsForUser(userId, CancellationToken.None);

        // Then: debe devolverse el registro persistido.
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.UserID, Is.EqualTo(userId));
        Assert.That(result.Value.SubscriptionID, Is.EqualTo(entity.SubscriptionID));
    }

    [Test]
    public async Task ShouldReturnNoSubscriptionFoundWhenUserHasNoSubscription()
    {
        // Given: un user id sin suscripciones registradas.
        string userIdWithoutSubscription = Guid.NewGuid().ToString();

        // When: se consulta la suscripcion para ese usuario.
        Result<SubscriptionEntity> result =
            await _repository.GetSubscriptionDetailsForUser(userIdWithoutSubscription, CancellationToken.None);

        // Then: debe mapear al error funcional de no suscripcion encontrada.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(SubscriptionErrors.NoSubscriptionFound));
    }

    [Test]
    public async Task ShouldMapFindOneByIndexCatchErrorToUnknownErrorWhenIndexIsInvalid()
    {
        // Given: un repositorio con indice invalido para forzar excepcion en Query/GetRemaining de FindOneByIndex.
        AWSConfig invalidConfig = new()
        {
            EmailTemplatesBucketName = _configuration.EmailTemplatesBucketName,
            UserCodesTable = _configuration.UserCodesTable,
            EmailTemplatesTable = _configuration.EmailTemplatesTable,
            UsersTable = _configuration.UsersTable,
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
            SubscriptionUserIdIndex = $"invalid-index-{Guid.NewGuid():N}",
            SettingsNameSpace = _configuration.SettingsNameSpace
        };

        SubscriptionRepository invalidRepository = new(
            invalidConfig,
            _dynamoDbContext,
            new SubscriptionEntityMapper(),
            CreateTestLogger<SubscriptionRepository>());

        // When: se consulta usando repositorio configurado con indice invalido.
        Result<SubscriptionEntity> result =
            await invalidRepository.GetSubscriptionDetailsForUser(Guid.NewGuid().ToString(), CancellationToken.None);

        // Then: el catch de FindOneByIndex debe terminar mapeado como UnknownError.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(AppErrors.UnknownError));
        Assert.That(result.Error, Is.Not.EqualTo(SubscriptionErrors.NoSubscriptionFound));
    }
}
