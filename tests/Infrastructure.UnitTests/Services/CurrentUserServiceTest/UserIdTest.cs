using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Moq;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure.UnitTests.Services.CurrentUserServiceTest;

[TestFixture]
public class UserIdTest
{
    [Test]
    public void ShouldReturnNullWhenHttpContextIsMissing()
    {
        // Given
        Mock<IHttpContextAccessor> accessorMock = new();
        accessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);
        CurrentUserService service = new(accessorMock.Object);

        // When
        string? result = service.UserId;

        // Then
        Assert.That(result, Is.Null);
    }

    [Test]
    public void ShouldReturnNullWhenNameIdentifierClaimIsMissing()
    {
        // Given
        DefaultHttpContext context = new();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Email, "user@test.com")
        }));

        Mock<IHttpContextAccessor> accessorMock = new();
        accessorMock.Setup(x => x.HttpContext).Returns(context);
        CurrentUserService service = new(accessorMock.Object);

        // When
        string? result = service.UserId;

        // Then
        Assert.That(result, Is.Null);
    }

    [Test]
    public void ShouldReturnUserIdWhenNameIdentifierClaimExists()
    {
        // Given
        DefaultHttpContext context = new();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-123")
        }));

        Mock<IHttpContextAccessor> accessorMock = new();
        accessorMock.Setup(x => x.HttpContext).Returns(context);
        CurrentUserService service = new(accessorMock.Object);

        // When
        string? result = service.UserId;

        // Then
        Assert.That(result, Is.EqualTo("user-123"));
    }
}
