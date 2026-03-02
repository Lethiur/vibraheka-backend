using CSharpFunctionalExtensions;
using Moq;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Infrastructure.UnitTests.Services.PaymentServiceTest;

[TestFixture]
public class GetUserByIDTest : GenericPaymentServiceTest
{
    [Test]
    public async Task ShouldReturnInvalidUserIdWhenUserIdIsNullOrWhitespace()
    {
        // Given

        // When
        Result<UserEntity> nullResult = await _service.GetUserByID(null!, CancellationToken.None);
        Result<UserEntity> emptyResult = await _service.GetUserByID("", CancellationToken.None);
        Result<UserEntity> whitespaceResult = await _service.GetUserByID("  ", CancellationToken.None);

        // Then
        Assert.That(nullResult.IsFailure, Is.True);
        Assert.That(emptyResult.IsFailure, Is.True);
        Assert.That(whitespaceResult.IsFailure, Is.True);
        Assert.That(nullResult.Error, Is.EqualTo(UserErrors.InvalidUserID));
        Assert.That(emptyResult.Error, Is.EqualTo(UserErrors.InvalidUserID));
        Assert.That(whitespaceResult.Error, Is.EqualTo(UserErrors.InvalidUserID));
        _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ShouldMapInfrastructureUserNotFoundError()
    {
        // Given
        _userRepositoryMock.Setup(x => x.GetByIdAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<UserEntity>(InfrastructureUserErrors.UserNotFound));

        // When
        Result<UserEntity> result = await _service.GetUserByID("user-1", CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.UserNotFound));
    }
}
