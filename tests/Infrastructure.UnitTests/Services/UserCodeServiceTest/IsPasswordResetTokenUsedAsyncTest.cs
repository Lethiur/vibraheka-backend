using CSharpFunctionalExtensions;
using Moq;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Infrastructure.UnitTests.Services.UserCodeServiceTest;

[TestFixture]
public class IsPasswordResetTokenUsedAsyncTest : GenericUserCodeServiceTest
{
    [Test]
    public async Task ShouldReturnTrueWhenRepositoryReturnsTokenEntity()
    {
        // Given: an existing replay marker for the provided token id
        const string email = "user@test.com";
        const string tokenId = "token-123";
        UserCodeRepositoryMock.Setup(x => x.GetCodeEntityByTokenId(tokenId, CancellationToken.None))
            .ReturnsAsync(Result.Success(new UserCodeEntity
            {
                UserEmail = email,
                Code = tokenId,
                ActionType = ActionType.PasswordReset
            }));

        // When: checking if the token is already used
        Result<bool> result = await Service.IsPasswordResetTokenUsedAsync(email, tokenId, CancellationToken.None);

        // Then: service returns true and repository is called with the token id
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.True);
        UserCodeRepositoryMock.Verify(x => x.GetCodeEntityByTokenId(tokenId, CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task ShouldReturnFalseWhenRepositoryReturnsNoRecordFound()
    {
        // Given: no replay marker for the provided token id
        const string email = "user@test.com";
        const string tokenId = "token-123";
        UserCodeRepositoryMock.Setup(x => x.GetCodeEntityByTokenId(tokenId, CancellationToken.None))
            .ReturnsAsync(Result.Failure<UserCodeEntity>(UserCodeErrors.NoRecordFound));

        // When: checking if the token is already used
        Result<bool> result = await Service.IsPasswordResetTokenUsedAsync(email, tokenId, CancellationToken.None);

        // Then: service compensates as token not used
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.False);
        UserCodeRepositoryMock.Verify(x => x.GetCodeEntityByTokenId(tokenId, CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task ShouldReturnFailureWhenRepositoryReturnsUnexpectedError()
    {
        // Given: repository fails with a non-compensated error
        const string email = "user@test.com";
        const string tokenId = "token-123";
        UserCodeRepositoryMock.Setup(x => x.GetCodeEntityByTokenId(tokenId, CancellationToken.None))
            .ReturnsAsync(Result.Failure<UserCodeEntity>(UserErrors.UnexpectedError));

        // When: checking if the token is already used
        Result<bool> result = await Service.IsPasswordResetTokenUsedAsync(email, tokenId, CancellationToken.None);

        // Then: the failure is propagated
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.UnexpectedError));
        UserCodeRepositoryMock.Verify(x => x.GetCodeEntityByTokenId(tokenId, CancellationToken.None), Times.Once);
    }
}
