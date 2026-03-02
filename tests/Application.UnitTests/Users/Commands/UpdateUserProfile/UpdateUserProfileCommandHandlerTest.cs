using NUnit.Framework;
using CSharpFunctionalExtensions;
using MediatR;
using Moq;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Users.Commands.UpdateUserProfile;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Models.Results.User;

namespace VibraHeka.Application.UnitTests.Users.Commands.UpdateUserProfile;

[TestFixture]
public class UpdateUserProfileCommandHandlerTest
{
    private Mock<ICurrentUserService> _currentUserServiceMock;
    private Mock<IUserService> _userServiceMock;
    private UpdateUserCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _userServiceMock = new Mock<IUserService>();
        _currentUserServiceMock.Setup(x => x.UserId).Returns("updater-id");

        _handler = new UpdateUserCommandHandler(_currentUserServiceMock.Object, _userServiceMock.Object);
    }

    [Test]
    public async Task ShouldUpdateUserWhenCommandIsValid()
    {
        // Given
        UserDTO dto = new()
        {
            Id = Guid.NewGuid().ToString(),
            Email = "user@test.com",
            FirstName = "John",
            MiddleName = "Middle",
            LastName = "Doe",
            Bio = "Bio",
            PhoneNumber = "+34911111222"
        };

        _userServiceMock.Setup(x => x.UpdateUserAsync(
                It.Is<UserEntity>(u =>
                    u.Id == dto.Id &&
                    u.Email == dto.Email &&
                    u.FirstName == dto.FirstName &&
                    u.MiddleName == dto.MiddleName &&
                    u.LastName == dto.LastName &&
                    u.Bio == dto.Bio &&
                    u.PhoneNumber == dto.PhoneNumber),
                "updater-id",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Unit.Value));

        UpdateUserProfileCommand command = new(dto);

        // When
        Result<Unit> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        _userServiceMock.Verify(x => x.UpdateUserAsync(It.IsAny<UserEntity>(), "updater-id", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ShouldReturnNotAuthorizedWhenNewUserDataIsNull()
    {
        // Given
        UpdateUserProfileCommand command = new(null!);

        // When
        Result<Unit> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.NotAuthorized));
        _userServiceMock.Verify(x => x.UpdateUserAsync(It.IsAny<UserEntity>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

