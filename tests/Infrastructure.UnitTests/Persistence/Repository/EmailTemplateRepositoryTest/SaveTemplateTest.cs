using System.ComponentModel;
using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using MediatR;
using Moq;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.EmailTemplateRepositoryTest;

[TestFixture]
public class SaveTemplateTest : GenericEmailTemplateRepositoryTest
{
    [Test]
    [DisplayName("Should save template and return success")]
    public async Task ShouldSaveTemplateAndReturnSuccess()
    {
        // Given
        EmailEntity template = new()
        {
            ID = "template-1",
            Name = "Welcome",
            Path = "templates/template-1/template.json",
            Created = DateTimeOffset.UtcNow.AddDays(-1),
            LastModified = DateTimeOffset.UtcNow
        };

        _contextMock
            .Setup(c => c.SaveAsync(
                It.IsAny<EmailTemplateDBModel>(),
                It.Is<SaveConfig>(cfg => cfg.OverrideTableName == TableName),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // When
        Result<Unit> result = await Repository.SaveTemplate(template, CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        _contextMock.Verify(c => c.SaveAsync(
                It.Is<EmailTemplateDBModel>(m => m.TemplateID == template.ID && m.Path == template.Path && m.Name == template.Name),
                It.Is<SaveConfig>(cfg => cfg.OverrideTableName == TableName),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    [DisplayName("Should return failure when DynamoDB save throws")]
    public async Task ShouldReturnFailureWhenDynamoDbSaveThrows()
    {
        // Given
        EmailEntity template = new()
        {
            ID = "template-2",
            Name = "Welcome",
            Path = "templates/template-2/template.json",
        };

        _contextMock
            .Setup(c => c.SaveAsync(It.IsAny<EmailTemplateDBModel>(), It.IsAny<SaveConfig>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Dynamo save error"));

        // When
        Result<Unit> result = await Repository.SaveTemplate(template, CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Does.Contain("Dynamo save error"));
    }
}

