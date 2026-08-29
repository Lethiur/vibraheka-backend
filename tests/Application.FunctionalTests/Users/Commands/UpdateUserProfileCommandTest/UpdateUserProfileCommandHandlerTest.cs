using CSharpFunctionalExtensions;
using MediatR;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Users.Commands.UpdateUserProfile;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Application.FunctionalTests.Users.Commands.UpdateUserProfileCommandTest;

[TestFixture]
public class UpdateUserProfileCommandHandlerTest
{
    private Mock<ICurrentUserService> _currentUserServiceMock = default!;
    private Mock<IUserService> _userServiceMock = default!;
    private UpdateUserCommandHandler _handler = default!;

    [SetUp]
    public void SetUp()
    {
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _userServiceMock = new Mock<IUserService>();
        _currentUserServiceMock.Setup(x => x.UserId).Returns("updater-id");
        _handler = new UpdateUserCommandHandler(_currentUserServiceMock.Object, _userServiceMock.Object);
    }

    [Test]
    public async Task ShouldUpdateUserWhenDataIsValid()
    {
        // Given
        UserEntity data = new()
        {
            Id = "updater-id",
            Email = "user@test.com",
            FirstName = "John",
            MiddleName = "Middle",
            LastName = "Doe",
            Bio = "Bio",
            PhoneNumber = "+34911111222"
        };

        _userServiceMock.Setup(x => x.UpdateUserAsync(It.IsAny<UserEntity>(), "updater-id", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Unit.Value));

        // When
        Result<Unit> result = await _handler.Handle(new UpdateUserProfileCommand(data.Email, data.FirstName, data.MiddleName, data.LastName, data.PhoneNumber, data.Bio, string.Empty, data.PhoneNumber), CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
    }
}

