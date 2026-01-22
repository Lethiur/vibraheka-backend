using CSharpFunctionalExtensions;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Settings.Queries.GetTemplateForAction;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.Settings;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Application.FunctionalTests.Settings.Queries.GetTemplateForAction;

[TestFixture]
public class GetAllTemplatesForActionTest
{
   private Mock<ISettingsService> SettingsServiceMock;
    private Mock<ICurrentUserService> CurrentUserServiceMock;
    private Mock<IPrivilegeService> PrivilegeServiceMock;
    private GetTemplatesForActionQueryHandler Handler;

    [SetUp]
    public void SetUp()
    {
        SettingsServiceMock = new Mock<ISettingsService>();
        CurrentUserServiceMock = new Mock<ICurrentUserService>();
        PrivilegeServiceMock = new Mock<IPrivilegeService>();

        Handler = new GetTemplatesForActionQueryHandler(
            SettingsServiceMock.Object,
            CurrentUserServiceMock.Object,
            PrivilegeServiceMock.Object);
    }

    [Test]
    public async Task ShouldReturnInvalidUserIDErrorWhenUserIdIsEmpty()
    {
        // Given
        CurrentUserServiceMock.Setup(x => x.UserId).Returns(string.Empty);
        var query = new GetTemplatesForActionQuery();

        // When
        Result<IEnumerable<TemplateForActionEntity>> result = await Handler.Handle(query, CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.InvalidUserID));
        PrivilegeServiceMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task ShouldReturnNotAuthorizedErrorWhenUserIsNotAdmin()
    {
        // Given
        const string userId = "user-123";
        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        PrivilegeServiceMock.Setup(x => x.HasRoleAsync(userId, UserRole.Admin))
            .ReturnsAsync(Result.Success(false));
        var query = new GetTemplatesForActionQuery();

        // When
        Result<IEnumerable<TemplateForActionEntity>> result = await Handler.Handle(query, CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.NotAuthorized));
        SettingsServiceMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task ShouldReturnTemplatesWhenUserIsAdminAndServiceSucceeds()
    {
        // Given
        const string userId = "admin-123";
        var expectedTemplates = new List<TemplateForActionEntity>
        {
            new() { TemplateID = "1", ActionType = ActionType.PasswordReset }
        };

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        PrivilegeServiceMock.Setup(x => x.HasRoleAsync(userId, UserRole.Admin))
            .ReturnsAsync(Result.Success(true));
        SettingsServiceMock.Setup(x => x.GetAllTemplatesForActions())
            .Returns(Result.Success<IEnumerable<TemplateForActionEntity>>(expectedTemplates));
        
        var query = new GetTemplatesForActionQuery();

        // When
        Result<IEnumerable<TemplateForActionEntity>> result = await Handler.Handle(query, CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(expectedTemplates));
        SettingsServiceMock.Verify(x => x.GetAllTemplatesForActions(), Times.Once);
    }

    [Test]
    public async Task ShouldReturnFailureWhenSettingsServiceFails()
    {
        // Given
        const string userId = "admin-123";
        const string errorMessage = "Database Error";

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        PrivilegeServiceMock.Setup(x => x.HasRoleAsync(userId, UserRole.Admin))
            .ReturnsAsync(Result.Success(true));
        SettingsServiceMock.Setup(x => x.GetAllTemplatesForActions())
            .Returns(Result.Failure<IEnumerable<TemplateForActionEntity>>(errorMessage));
        
        var query = new GetTemplatesForActionQuery();

        // When
        Result<IEnumerable<TemplateForActionEntity>> result = await Handler.Handle(query, CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(errorMessage));
    }
}
