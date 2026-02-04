using System.ComponentModel;
using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using Moq;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.EmailTemplateRepositoryTest;

[TestFixture]
public class GetAllTemplatesTest : GenericEmailTemplateRepositoryTest
{
    [Test]
    [DisplayName("Should return all templates mapped to domain entities")]
    public async Task ShouldReturnAllTemplatesMappedToDomainEntities()
    {
        // Given
        List<EmailTemplateDBModel> models =
        [
            new EmailTemplateDBModel { TemplateID = "t1", Name = "n1", Path = "p1" },
            new EmailTemplateDBModel { TemplateID = "t2", Name = "n2", Path = "p2" }
        ];

        Mock<IAsyncSearch<EmailTemplateDBModel>> searchMock = new();
        searchMock
            .Setup(s => s.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(models);

        _contextMock
            .Setup(c => c.ScanAsync<EmailTemplateDBModel>(It.IsAny<IEnumerable<ScanCondition>>(),
                It.Is<ScanConfig>(cfg => cfg.OverrideTableName == TableName)))
            .Returns(searchMock.Object);

        // When
        Result<IEnumerable<VibraHeka.Domain.Entities.EmailEntity>> result = await Repository.GetAllTemplates(CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Has.Exactly(1).Matches<VibraHeka.Domain.Entities.EmailEntity>(e => e.ID == "t1" && e.Name == "n1" && e.Path == "p1"));
        Assert.That(result.Value, Has.Exactly(1).Matches<VibraHeka.Domain.Entities.EmailEntity>(e => e.ID == "t2" && e.Name == "n2" && e.Path == "p2"));
    }

    [Test]
    [DisplayName("Should return failure when scan throws exception")]
    public async Task ShouldReturnFailureWhenScanThrowsException()
    {
        _contextMock
            .Setup(c => c.ScanAsync<EmailTemplateDBModel>(It.IsAny<IEnumerable<ScanCondition>>(), It.IsAny<ScanConfig>()))
            .Throws(new Exception("Scan failed"));

        Result<IEnumerable<VibraHeka.Domain.Entities.EmailEntity>> result = await Repository.GetAllTemplates(CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Does.Contain("Scan failed"));
    }
}

