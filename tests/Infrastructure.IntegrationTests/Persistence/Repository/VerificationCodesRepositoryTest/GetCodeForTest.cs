using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.VerificationCodesRepositoryTest;

[TestFixture]
public class GetCodeForTest : GenericVerificationCodesRepositoryIntegrationTest
{
    [Test]
    public async Task ShouldReturnVerificationCodeWhenItExists()
    {
        // Given
        string email = $"{Guid.NewGuid():N}@test.com";
        VerificationCodeDBModel model = new()
        {
            UserName = email,
            Code = "123456",
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

#if DEBUG
        await _dynamoDbContext.SaveAsync(model, new SaveConfig
        {
            OverrideTableName = _configuration.CodesTable
        }, CancellationToken.None);
#endif

        // When
        Result<VerificationCodeEntity> result = await _repository.GetCodeFor(email);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.UserName, Is.EqualTo(email));
        Assert.That(result.Value.Code, Is.EqualTo("123456"));
    }

    [Test]
    public async Task ShouldReturnFailureWhenCodeDoesNotExist()
    {
        // Given
        string email = $"{Guid.NewGuid():N}@test.com";

        // When
        Result<VerificationCodeEntity> result = await _repository.GetCodeFor(email);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Does.Contain("No codes found for user with Email"));
    }
}
