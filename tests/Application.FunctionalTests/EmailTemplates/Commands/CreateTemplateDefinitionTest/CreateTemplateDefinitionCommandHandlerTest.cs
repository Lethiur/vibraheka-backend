using CSharpFunctionalExtensions;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.EmailTemplates.Commands.CreateTemplateDefinition;
using VibraHeka.Domain.EmailTemplates.Ports.Out;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.User.Services;

namespace VibraHeka.Application.FunctionalTests.EmailTemplates.Commands.CreateTemplateDefinitionTest;

[TestFixture]
public class CreateTemplateDefinitionCommandHandlerTest
{
    private Mock<ICurrentUserService> _currentUserServiceMock = default!;
    private Mock<EmailTemplatePort> _templatesServiceMock = default!;
    private CreateTemplateDefinitionCommandHandler _handler = default!;

    [SetUp]
    public void SetUp()
    {
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _templatesServiceMock = new Mock<EmailTemplatePort>();
        _handler = new CreateTemplateDefinitionCommandHandler(_currentUserServiceMock.Object, _templatesServiceMock.Object);
    }

    [Test]
    public async Task ShouldCreateEntityAndCallSaveEmailTemplate()
    {
        const string userId = "admin-1";
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        CreateTemplateDefinitionCommand command = new("Welcome Template");
        _templatesServiceMock
            .Setup(x => x.SaveEmailTemplate(It.IsAny<EmailTemplateEntity>(), CancellationToken.None))
            .ReturnsAsync(Result.Success("template-id"));

        Result<string> result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo("template-id"));

        Guid a;
        
        _templatesServiceMock.Verify(x => x.SaveEmailTemplate(
            It.Is<EmailTemplateEntity>(e =>
                !string.IsNullOrWhiteSpace(e.TemplateID) &&
                Guid.TryParse(e.TemplateID, out a) &&
                e.Name == command.TempateName &&
                e.CreatedBy == userId),
            CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task ShouldReturnFailureWhenServiceFails()
    {
        _currentUserServiceMock.Setup(x => x.UserId).Returns("admin-1");
        CreateTemplateDefinitionCommand command = new("Welcome Template");
        _templatesServiceMock
            .Setup(x => x.SaveEmailTemplate(It.IsAny<EmailTemplateEntity>(), CancellationToken.None))
            .ReturnsAsync(Result.Failure<string>("ET-FAIL"));

        Result<string> result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("ET-FAIL"));
    }
}

