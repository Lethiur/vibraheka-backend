using System.ComponentModel;
using CSharpFunctionalExtensions;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Admin.Commands.CreateTherapist;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Common.Interfaces;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Application.UnitTests.Admin.Commands.CreateTherapist;

[TestFixture]
public class CreateTherapistCommandHandlerTests
{
    private Mock<ICognitoService> CognitoServiceMock;
    private Mock<IUserRepository> RepositoryMock;
    private Mock<ICurrentUserService> CurrentUserServiceMock;
    private Mock<IPrivilegeService> PrivilegeServiceMock;
    private CreateTherapistCommandHandler Handler;

    [SetUp]
    public void SetUp()
    {
        CognitoServiceMock = new Mock<ICognitoService>();
        RepositoryMock = new Mock<IUserRepository>();
        CurrentUserServiceMock = new Mock<ICurrentUserService>();
        PrivilegeServiceMock = new Mock<IPrivilegeService>();

        Handler = new CreateTherapistCommandHandler(
            CognitoServiceMock.Object,
            RepositoryMock.Object,
            CurrentUserServiceMock.Object,
            PrivilegeServiceMock.Object);
    }

    #region Privilege Validation Tests

    [Test]
    [DisplayName("Should return failure when current user is not an admin")]
    public async Task ShouldReturnFailureWhenUserIsNotAdmin()
    {
        // Given: A command to create a therapist and a user without admin privileges
        CreateTherapistCommand command = new CreateTherapistCommand("test@therapist.com", "Dr. Smith");
        string userId = "non-admin-id";
        
        CurrentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        PrivilegeServiceMock.Setup(x => x.HasRoleAsync(userId, UserRole.Admin))
            .ReturnsAsync(Result.Success(false));

        // When: Handling the command
        Result<string> result = await Handler.Handle(command, CancellationToken.None);

        // Then: Should return not authorized failure and not call external services
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserException.NotAuthorized));
        
        CognitoServiceMock.Verify(x => x.RegisterUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        RepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Test]
    [DisplayName("Should return failure when current user ID is null")]
    public async Task ShouldReturnFailureWhenUserIdIsNull()
    {
        // Given: A command and no authenticated user context
        CreateTherapistCommand command = new CreateTherapistCommand("test@therapist.com", "Dr. Smith");
        
        CurrentUserServiceMock.Setup(x => x.UserId).Returns((string?)null);
        PrivilegeServiceMock.Setup(x => x.HasRoleAsync("", UserRole.Admin))
            .ReturnsAsync(Result.Success(false));

        // When: Handling the command
        Result<string> result = await Handler.Handle(command, CancellationToken.None);

        // Then: Should return not authorized failure
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserException.NotAuthorized));
        
        CognitoServiceMock.Verify(x => x.RegisterUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region Therapist Creation Tests

    [Test]
    [DisplayName("Should return success and user id when therapist is created successfully")]
    public async Task ShouldReturnSuccessWhenTherapistIsCreated()
    {
        // Given: A valid command and an admin user with successful service responses
        CreateTherapistCommand command = new CreateTherapistCommand("test@therapist.com", "Dr. Smith");
        string adminId = "admin-id";
        string cognitoId = "new-cognito-id";

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(adminId);
        PrivilegeServiceMock.Setup(x => x.HasRoleAsync(adminId, UserRole.Admin))
            .ReturnsAsync(Result.Success(true));
        
        CognitoServiceMock.Setup(x => x.RegisterUserAsync(command.Email, It.IsAny<string>(), command.Name))
            .ReturnsAsync(Result.Success(cognitoId));

        RepositoryMock.Setup(x => x.AddAsync(It.Is<User>(u => 
                u.Email == command.Email && 
                u.FullName == command.Name && 
                u.Role == UserRole.Therapist &&
                u.Id == cognitoId &&
                u.CognitoId == cognitoId)))
            .ReturnsAsync(Result.Success(cognitoId));

        // When: Handling the command
        Result<string> result = await Handler.Handle(command, CancellationToken.None);

        // Then: Should return success with the new ID and verify all steps were called
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(cognitoId));
        
        PrivilegeServiceMock.Verify(x => x.HasRoleAsync(adminId, UserRole.Admin), Times.Once);
        CognitoServiceMock.Verify(x => x.RegisterUserAsync(command.Email, It.IsAny<string>(), command.Name), Times.Once);
        RepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
    }

    [Test]
    [DisplayName("Should return failure when Cognito registration fails")]
    public async Task ShouldReturnFailureWhenCognitoRegistrationFails()
    {
        // Given: An admin user but Cognito registration fails
        CreateTherapistCommand command = new CreateTherapistCommand("test@therapist.com", "Dr. Smith");
        string adminId = "admin-id";
        string errorMessage = "Cognito registration error";

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(adminId);
        PrivilegeServiceMock.Setup(x => x.HasRoleAsync(adminId, UserRole.Admin))
            .ReturnsAsync(Result.Success(true));

        CognitoServiceMock.Setup(x => x.RegisterUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Result.Failure<string>(errorMessage));

        // When: Handling the command
        Result<string> result = await Handler.Handle(command, CancellationToken.None);

        // Then: Should return failure and not attempt to save to repository
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(errorMessage));
        
        RepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Test]
    [DisplayName("Should return failure when database persistence fails")]
    public async Task ShouldReturnFailureWhenRepositoryFails()
    {
        // Given: Successful Cognito registration but database failure
        CreateTherapistCommand command = new CreateTherapistCommand("test@therapist.com", "Dr. Smith");
        string adminId = "admin-id";
        string dbError = "Error saving user to DB";

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(adminId);
        PrivilegeServiceMock.Setup(x => x.HasRoleAsync(adminId, UserRole.Admin))
            .ReturnsAsync(Result.Success(true));

        CognitoServiceMock.Setup(x => x.RegisterUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Result.Success("cognito-id"));

        RepositoryMock.Setup(x => x.AddAsync(It.IsAny<User>()))
            .ReturnsAsync(Result.Failure<string>(dbError));

        // When: Handling the command
        Result<string> result = await Handler.Handle(command, CancellationToken.None);

        // Then: Should return failure from the repository layer
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(dbError));
        
        CognitoServiceMock.Verify(x => x.RegisterUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        RepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
    }

    #endregion
}
