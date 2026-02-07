using CSharpFunctionalExtensions;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Users.Commands.AdminCreateTherapist;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Models.Results.User;

namespace VibraHeka.Application.FunctionalTests.Users;

[TestFixture]
public class CreateTherapistCommandHandlerTest
{
    private Mock<IUserService> _userServiceMock = default!;
    private Mock<IUserRepository> _userRepositoryMock = default!;
    private Mock<ICurrentUserService> _currentUserServiceMock = default!;
    private CreateTherapistCommandHandler _handler = default!;

    [SetUp]
    public void SetUp()
    {
        _userServiceMock = new Mock<IUserService>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _handler = new CreateTherapistCommandHandler(_userServiceMock.Object, _userRepositoryMock.Object, _currentUserServiceMock.Object);
    }

    [Test]
    public async Task ShouldCreateTherapistWithTherapistRole()
    {
        // Given
        _currentUserServiceMock.Setup(x => x.UserId).Returns("admin-1");
        CreateTherapistCommand command = new CreateTherapistCommand(new UserDTO(){Email = "test@therapist.com", FirstName = "Dr. Smith"});

        _userServiceMock
            .Setup(x => x.RegisterUserAsync(command.TherapistData.Email, It.IsAny<string>(), command.TherapistData.FirstName))
            .ReturnsAsync(Result.Success("new-user-id"));

        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<UserEntity>()))
            .ReturnsAsync(Result.Success("new-user-id"));

        // When
        Result<string> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo("new-user-id"));
        _userRepositoryMock.Verify(x => x.AddAsync(It.Is<UserEntity>(u =>
            u.Id == "new-user-id" &&
            u.CognitoId == "new-user-id" &&
            u.Email == command.TherapistData.Email &&
            u.FirstName == command.TherapistData.FirstName &&
            u.Role == UserRole.Therapist &&
            u.CreatedBy == "admin-1" &&
            u.LastModifiedBy == "admin-1")), Times.Once);
    }

    [Test]
    public async Task ShouldReturnFailureWhenCognitoFails()
    {
        // Given
        _currentUserServiceMock.Setup(x => x.UserId).Returns("admin-1");
        CreateTherapistCommand command = new CreateTherapistCommand(new UserDTO(){Email = "test@therapist.com", FirstName = "Dr. Smith"});

        _userServiceMock
            .Setup(x => x.RegisterUserAsync(command.TherapistData.Email, It.IsAny<string>(), command.TherapistData.FirstName))
            .ReturnsAsync(Result.Failure<string>("E-002"));

        // When
        Result<string> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("E-002"));
        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<UserEntity>()), Times.Never);
    }

    [Test]
    public async Task ShouldReturnFailureWhenRepositoryFails()
    {
        // Given
        _currentUserServiceMock.Setup(x => x.UserId).Returns("admin-1");
        CreateTherapistCommand command = new CreateTherapistCommand(new UserDTO(){Email = "test@therapist.com", FirstName = "Dr. Smith"});

        _userServiceMock
            .Setup(x => x.RegisterUserAsync(command.TherapistData.Email, It.IsAny<string>(), command.TherapistData.FirstName))
            .ReturnsAsync(Result.Success("new-user-id"));

        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<UserEntity>()))
            .ReturnsAsync(Result.Failure<string>("DB-FAIL"));

        // When
        Result<string> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("DB-FAIL"));
    }
}

