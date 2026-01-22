using CSharpFunctionalExtensions;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.EmailTemplates.Queries.GetAllEmailTemplates;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.EmailTemplates;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Application.FunctionalTests.EmailTemplates.Queries.GetAllEmailTemplatesTest;

[TestFixture]
public class GetAllEmailTemplatesQueryHandlerTest
{
    private Mock<ICurrentUserService> CurrentUserServiceMock;
    private Mock<IPrivilegeService> PrivilegeServiceMock;
    private Mock<IEmailTemplatesService> EmailTemplatesServiceMock;
    private GetAllEmailTemplatesQueryHandler Handler;

    [SetUp]
    public void SetUp()
    {
        CurrentUserServiceMock = new Mock<ICurrentUserService>();
        PrivilegeServiceMock = new Mock<IPrivilegeService>();
        EmailTemplatesServiceMock = new Mock<IEmailTemplatesService>();

        Handler = new GetAllEmailTemplatesQueryHandler(
            CurrentUserServiceMock.Object,
            PrivilegeServiceMock.Object,
            EmailTemplatesServiceMock.Object);
    }

    [Test]
    public async Task ShouldHandleInvalidUserID()
    {
        // Given
        CurrentUserServiceMock.Setup(x => x.UserId).Returns(string.Empty);
        GetAllEmailTemplatesQuery query = new GetAllEmailTemplatesQuery();

        // When
        Result<IEnumerable<EmailEntity>> result = await Handler.Handle(query, CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.InvalidUserID));
    }

    [Test]
    public async Task ShouldReturnNotAuthorizedErrorIfUserIsNotAdmin()
    {
        // Given
        const string userId = "user-123";
        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        PrivilegeServiceMock.Setup(x => x.HasRoleAsync(userId, UserRole.Admin))
            .ReturnsAsync(false);
        GetAllEmailTemplatesQuery query = new GetAllEmailTemplatesQuery();

        // When
        Result<IEnumerable<EmailEntity>> result = await Handler.Handle(query, CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.NotAuthorized));
    }

    [Test]
    public async Task ShouldReturnTemplatesIfEverythingIsOk()
    {
        // Given
        const string userId = "admin-123";
        IEnumerable<EmailEntity> templates = new List<EmailEntity> { new EmailEntity { ID = "1", Name = "Welcome" } };

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        PrivilegeServiceMock.Setup(x => x.HasRoleAsync(userId, UserRole.Admin))
            .ReturnsAsync(true);
        EmailTemplatesServiceMock.Setup(x => x.GetAllTemplates(CancellationToken.None))
            .ReturnsAsync(Result.Success(templates));
        GetAllEmailTemplatesQuery query = new GetAllEmailTemplatesQuery();

        // When
        Result<IEnumerable<EmailEntity>> result = await Handler.Handle(query, CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(templates));
        EmailTemplatesServiceMock.Verify(x => x.GetAllTemplates(CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task ShouldReturnFailureIfServiceFails()
    {
        // Given
        const string userId = "admin-123";
        const string errorMessage = "Error fetching from DynamoDB";

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        PrivilegeServiceMock.Setup(x => x.HasRoleAsync(userId, UserRole.Admin))
            .ReturnsAsync(true);
        EmailTemplatesServiceMock.Setup(x => x.GetAllTemplates(CancellationToken.None))
            .ReturnsAsync(Result.Failure<IEnumerable<EmailEntity>>(errorMessage));
        GetAllEmailTemplatesQuery query = new GetAllEmailTemplatesQuery();

        // When
        Result<IEnumerable<EmailEntity>> result = await Handler.Handle(query, CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(errorMessage));
    }
}
