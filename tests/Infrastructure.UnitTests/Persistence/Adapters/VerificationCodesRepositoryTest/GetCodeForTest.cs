using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;
using Moq;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.VerificationCodesRepositoryTest;

[TestFixture]
public class GetCodeForTest : GenericVerificationCodesRepositoryTest
{
    [Test]
    public async Task ShouldReturnMappedCodeWhenRecordExists()
    {
        // Given
        VerificationCodeDBModel dbModel = new()
        {
            UserName = "mail@test.com",
            Code = "123456",
            Timestamp = 1111
        };

        ContextMock.Setup(x => x.LoadAsync<VerificationCodeDBModel>("mail@test.com", It.IsAny<LoadConfig>()))
            .ReturnsAsync(dbModel);

        // When
        Result<VerificationCodeEntity> result = await Repository.GetCodeFor("mail@test.com");

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.UserName, Is.EqualTo("mail@test.com"));
        Assert.That(result.Value.Code, Is.EqualTo("123456"));
    }

    [Test]
    public async Task ShouldReturnFailureWhenRecordDoesNotExist()
    {
        // Given
        ContextMock.Setup(x => x.LoadAsync<VerificationCodeDBModel>("mail@test.com", It.IsAny<LoadConfig>()))
            .ReturnsAsync((VerificationCodeDBModel)null!);

        // When
        Result<VerificationCodeEntity> result = await Repository.GetCodeFor("mail@test.com");

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("No codes found for user with Email: mail@test.com"));
    }
}
