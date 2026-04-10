using CSharpFunctionalExtensions;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Users.Commands.RefreshToken;

namespace VibraHeka.Application.FunctionalTests.Users.Commands.RefreshTokenCommandTest;

[TestFixture]
public class RefreshTokenCommandTest
{
    private Mock<IUserService> UserServiceMock = default!;
    private RefreshTokenCommandHandler Handler = default!;

    [SetUp]
    public void SetUp()
    {
        UserServiceMock = new Mock<IUserService>();
        Handler = new RefreshTokenCommandHandler(UserServiceMock.Object);
    }

    [Test]
    [Description("Given a valid refresh token request, when the service succeeds, then it should return the refreshed token")]
    public async Task ShouldReturnRefreshedTokenWhenServiceSucceeds()
    {
        // Given: A valid refresh token request
        RefreshTokenCommand command = new("mock-refresh-token-value", "user@test.com");
        CancellationToken cancellationToken = new();
        const string refreshedToken = "new-access-token";

        UserServiceMock
            .Setup(x => x.RefreshToken(command.RefreshToken, command.Email, cancellationToken))
            .ReturnsAsync(Result.Success(refreshedToken));

        // When: The command is handled
        Result<string> result = await Handler.Handle(command, cancellationToken);

        // Then: The result should be successful and contain the refreshed token
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(refreshedToken));
        UserServiceMock.Verify(x => x.RefreshToken(command.RefreshToken, command.Email, cancellationToken), Times.Once);
    }

    [Test]
    [Description("Given a refresh token request, when the service fails, then it should propagate the failure")]
    public async Task ShouldReturnFailureWhenServiceFails()
    {
        // Given: A valid refresh token request
        RefreshTokenCommand command = new("mock-refresh-token-value", "user@test.com");
        CancellationToken cancellationToken = new();

        UserServiceMock
            .Setup(x => x.RefreshToken(command.RefreshToken, command.Email, cancellationToken))
            .ReturnsAsync(Result.Failure<string>("E-REFRESH"));

        // When: The command is handled
        Result<string> result = await Handler.Handle(command, cancellationToken);

        // Then: The result should be a failure with the expected error
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("E-REFRESH"));
        UserServiceMock.Verify(x => x.RefreshToken(command.RefreshToken, command.Email, cancellationToken), Times.Once);
    }
}
