using NUnit.Framework;
using CSharpFunctionalExtensions;
using Moq;
using VibraHeka.Application.Users.Queries.GetProfile;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Models.Results.User;

namespace VibraHeka.Application.UnitTests.Users.Queries.GetProfile;

[TestFixture]
public class GetUserProfileQueryHandlerTest
{
    private Mock<ICurrentUserService> _currentUserServiceMock;
    private Mock<IUserService> _userServiceMock;
    private GetUserProfileQueryHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _userServiceMock = new Mock<IUserService>();
        _handler = new GetUserProfileQueryHandler(_currentUserServiceMock.Object, _userServiceMock.Object);
    }

    [Test]
    public async Task ShouldReturnPhoneNumberWhenRequestingOwnProfile()
    {
        // Given
        string userId = Guid.NewGuid().ToString();
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        _userServiceMock.Setup(x => x.GetUserByID(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new UserEntity
            {
                Id = userId,
                Email = "user@test.com",
                FirstName = "John",
                PhoneNumber = "+34911111222"
            }));

        // When
        Result<UserDTO> result = await _handler.Handle(new GetUserProfileQuery(userId), CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.PhoneNumber, Is.EqualTo("+34911111222"));
        _userServiceMock.Verify(x => x.GetUserByID(userId, It.IsAny<CancellationToken>()), Times.Once);
        _currentUserServiceMock.VerifyGet(x => x.UserId, Times.Once);
    }

    [Test]
    public async Task ShouldHidePhoneNumberWhenRequestingOtherUserProfile()
    {
        // Given
        string currentUserId = Guid.NewGuid().ToString();
        string targetUserId = Guid.NewGuid().ToString();
        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUserId);

        _userServiceMock.Setup(x => x.GetUserByID(targetUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new UserEntity
            {
                Id = targetUserId,
                Email = "other@test.com",
                FirstName = "Other",
                PhoneNumber = "+34999999999"
            }));

        // When
        Result<UserDTO> result = await _handler.Handle(new GetUserProfileQuery(targetUserId), CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.PhoneNumber, Is.EqualTo(string.Empty));
        _userServiceMock.Verify(x => x.GetUserByID(targetUserId, It.IsAny<CancellationToken>()), Times.Once);
        _currentUserServiceMock.VerifyGet(x => x.UserId, Times.Once);
    }
}

