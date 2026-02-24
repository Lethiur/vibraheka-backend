using NUnit.Framework;
using CSharpFunctionalExtensions;
using Moq;
using VibraHeka.Application.Users.Queries.GetProfile;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Models.Results.User;

namespace VibraHeka.Application.FunctionalTests.Users.Queries.GetUserProfileQueryTest;

[TestFixture]
public class GetUserProfileQueryHandlerTest
{
    private Mock<ICurrentUserService> _currentUserServiceMock = default!;
    private Mock<IUserService> _userServiceMock = default!;
    private GetUserProfileQueryHandler _handler = default!;

    [SetUp]
    public void SetUp()
    {
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _userServiceMock = new Mock<IUserService>();
        _handler = new GetUserProfileQueryHandler(_currentUserServiceMock.Object, _userServiceMock.Object);
    }

    [Test]
    public async Task ShouldHidePhoneNumberWhenCurrentUserIsDifferent()
    {
        // Given
        string currentUserId = Guid.NewGuid().ToString();
        string targetUserId = Guid.NewGuid().ToString();
        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUserId);
        _userServiceMock.Setup(x => x.GetUserByID(targetUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new UserEntity { Id = targetUserId, PhoneNumber = "+34999999999" }));

        // When
        Result<UserDTO> result = await _handler.Handle(new GetUserProfileQuery(targetUserId), CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.PhoneNumber, Is.EqualTo(string.Empty));
    }
}

