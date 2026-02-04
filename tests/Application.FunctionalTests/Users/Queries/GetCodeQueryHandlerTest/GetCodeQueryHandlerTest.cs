using CSharpFunctionalExtensions;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Users.Queries.GetCode;
using VibraHeka.Domain.Common.Interfaces.Codes;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Application.FunctionalTests.Users;

[TestFixture]
public class GetCodeQueryHandlerTest
{
    private Mock<ICodeRepository> _repoMock = default!;
    private GetCodeQueryHandler _handler = default!;

    [SetUp]
    public void SetUp()
    {
        _repoMock = new Mock<ICodeRepository>();
        _handler = new GetCodeQueryHandler(_repoMock.Object);
    }

    [Test]
    [Description("Given a valid username, when the repository returns a verification code, then the handler should return that code")]
    public async Task ShouldReturnCodeWhenRepositorySucceeds()
    {
        // Given
        GetCodeQuery query = new("user@test.com");
        VerificationCodeEntity entity = new() { Code = "123456" };
        _repoMock.Setup(x => x.GetCodeFor(query.UserName)).ReturnsAsync(Result.Success(entity));

        // When
        Result<VerificationCodeEntity> result = await _handler.Handle(query, CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Code, Is.EqualTo("123456"));
    }

    [Test]
    [Description("Given a username, when the repository fails to find a code, then the handler should return a failure")]
    public async Task ShouldReturnFailureWhenRepositoryFails()
    {
        // Given
        GetCodeQuery query = new("user@test.com");
        string errorMessage = "CODE-NOT-FOUND";
        _repoMock.Setup(x => x.GetCodeFor(query.UserName)).ReturnsAsync(Result.Failure<VerificationCodeEntity>(errorMessage));

        // When
        Result<VerificationCodeEntity> result = await _handler.Handle(query, CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(errorMessage));
    }
}

