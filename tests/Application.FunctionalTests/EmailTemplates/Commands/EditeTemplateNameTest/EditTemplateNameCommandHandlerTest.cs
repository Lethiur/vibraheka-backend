using CSharpFunctionalExtensions;
using MediatR;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.EmailTemplates.Commands.EditTemplateName;
using VibraHeka.Domain.Common.Interfaces.EmailTemplates;

namespace VibraHeka.Application.FunctionalTests.EmailTemplates.Commands;

[TestFixture]
public class EditTemplateNameCommandHandlerTest
{
    private Mock<IEmailTemplatesService> _templatesServiceMock = default!;
    private EditTemplateNameCommandHandler _handler = default!;

    [SetUp]
    public void SetUp()
    {
        _templatesServiceMock = new Mock<IEmailTemplatesService>();
        _handler = new EditTemplateNameCommandHandler(_templatesServiceMock.Object);
    }

    [Test]
    public async Task ShouldReturnSuccessWhenServiceSucceeds()
    {
        EditTemplateNameCommand command = new("template-1", "New Name");
        _templatesServiceMock
            .Setup(x => x.EditTemplateName(command.TemplateID, command.NewTemplateName, CancellationToken.None))
            .ReturnsAsync(Result.Success(Unit.Value));

        Result<Unit> result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        _templatesServiceMock.Verify(
            x => x.EditTemplateName(command.TemplateID, command.NewTemplateName, CancellationToken.None),
            Times.Once);
    }

    [Test]
    public async Task ShouldReturnFailureWhenServiceFails()
    {
        EditTemplateNameCommand command = new("template-1", "New Name");
        _templatesServiceMock
            .Setup(x => x.EditTemplateName(command.TemplateID, command.NewTemplateName, CancellationToken.None))
            .ReturnsAsync(Result.Failure<Unit>("ET-FAIL"));

        Result<Unit> result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("ET-FAIL"));
    }
}

