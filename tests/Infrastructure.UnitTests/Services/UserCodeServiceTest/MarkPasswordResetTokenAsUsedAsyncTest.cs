using CSharpFunctionalExtensions;
using MediatR;
using Moq;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Infrastructure.UnitTests.Services.UserCodeServiceTest;

[TestFixture]
public class MarkPasswordResetTokenAsUsedAsyncTest : GenericUserCodeServiceTest
{
    [Test]
    public async Task ShouldSaveReplayMarkerWithExpectedFieldsWhenMarkingTokenAsUsed()
    {
        // Given: a token marker request with email, token id and expiration
        const string email = "user@test.com";
        const string tokenId = "token-123";
        DateTimeOffset expiresAt = DateTimeOffset.UtcNow.AddMinutes(15);

        UserCodeRepositoryMock.Setup(x => x.SaveCode(It.IsAny<UserCodeEntity>(), CancellationToken.None))
            .ReturnsAsync(Result.Success(Unit.Value));

        // When: marking the token as used
        Result<Unit> result = await Service.MarkPasswordResetTokenAsUsedAsync(email, tokenId, expiresAt, CancellationToken.None);

        // Then: save is invoked with the expected marker data
        Assert.That(result.IsSuccess, Is.True);
        UserCodeRepositoryMock.Verify(x => x.SaveCode(
            It.Is<UserCodeEntity>(entity =>
                entity.UserEmail == email &&
                entity.Code == tokenId &&
                entity.ActionType == ActionType.PasswordReset &&
                entity.ExpiresAtUnix == expiresAt.ToUnixTimeSeconds() &&
                entity.CreatedBy == email &&
                entity.LastModifiedBy == email),
            CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task ShouldReturnFailureWhenRepositoryCannotPersistReplayMarker()
    {
        // Given: a repository persistence failure
        const string email = "user@test.com";
        const string tokenId = "token-123";
        DateTimeOffset expiresAt = DateTimeOffset.UtcNow.AddMinutes(15);
        UserCodeRepositoryMock.Setup(x => x.SaveCode(It.IsAny<UserCodeEntity>(), CancellationToken.None))
            .ReturnsAsync(Result.Failure<Unit>("E-999"));

        // When: marking the token as used
        Result<Unit> result = await Service.MarkPasswordResetTokenAsUsedAsync(email, tokenId, expiresAt, CancellationToken.None);

        // Then: the failure is propagated
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("E-999"));
        UserCodeRepositoryMock.Verify(x => x.SaveCode(
            It.Is<UserCodeEntity>(entity =>
                entity.UserEmail == email &&
                entity.Code == tokenId &&
                entity.ActionType == ActionType.PasswordReset),
            CancellationToken.None), Times.Once);
    }
}
