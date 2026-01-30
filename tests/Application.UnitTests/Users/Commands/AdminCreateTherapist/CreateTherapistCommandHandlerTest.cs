using System.ComponentModel;
using CSharpFunctionalExtensions;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Users.Commands.AdminCreateTherapist;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Application.UnitTests.Users.Commands.AdminCreateTherapist;

[TestFixture]
public class CreateTherapistCommandHandlerTests
{
    private Mock<IUserService> CognitoServiceMock;
    private Mock<IUserRepository> RepositoryMock;
    private Mock<ICurrentUserService> CurrentUserServiceMock;
    private Mock<IPrivilegeService> PrivilegeServiceMock;
    private CreateTherapistCommandHandler Handler;

    [SetUp]
    public void SetUp()
    {
        CognitoServiceMock = new Mock<IUserService>();
        RepositoryMock = new Mock<IUserRepository>();
        CurrentUserServiceMock = new Mock<ICurrentUserService>();
        PrivilegeServiceMock = new Mock<IPrivilegeService>();

        Handler = new CreateTherapistCommandHandler(
            CognitoServiceMock.Object,
            RepositoryMock.Object,
            CurrentUserServiceMock.Object);
    }

    
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
