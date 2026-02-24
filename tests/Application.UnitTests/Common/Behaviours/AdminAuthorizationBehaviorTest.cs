using CSharpFunctionalExtensions;
using MediatR;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Common.Behaviours;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Application.UnitTests.Common.Behaviours;

public record TestAdminRequest() : IRequest<string>, IRequireAdmin;

[TestFixture]
public class AdminAuthorizationBehaviorTest
{
    [Test]
    public async Task ShouldCallNextWhenCurrentUserIsAdmin()
    {
        // Given
        Mock<ICurrentUserService> currentUserServiceMock = new();
        Mock<IPrivilegeService> privilegeServiceMock = new();
        currentUserServiceMock.Setup(x => x.UserId).Returns("admin-id");
        privilegeServiceMock.Setup(x => x.HasRoleAsync("admin-id", UserRole.Admin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(true));

        AdminAuthorizationBehavior<TestAdminRequest, string> behaviour =
            new(currentUserServiceMock.Object, privilegeServiceMock.Object);

        // When
        string result = await behaviour.Handle(new TestAdminRequest(), _ => Task.FromResult("ok"), CancellationToken.None);

        // Then
        Assert.That(result, Is.EqualTo("ok"));
    }

    [Test]
    public void ShouldThrowUnauthorizedExceptionWhenCurrentUserIsNotAdmin()
    {
        // Given
        Mock<ICurrentUserService> currentUserServiceMock = new();
        Mock<IPrivilegeService> privilegeServiceMock = new();
        currentUserServiceMock.Setup(x => x.UserId).Returns("user-id");
        privilegeServiceMock.Setup(x => x.HasRoleAsync("user-id", UserRole.Admin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(false));

        AdminAuthorizationBehavior<TestAdminRequest, string> behaviour =
            new(currentUserServiceMock.Object, privilegeServiceMock.Object);

        // When
        TestDelegate action = () => behaviour.Handle(new TestAdminRequest(), _ => Task.FromResult("ok"), CancellationToken.None).GetAwaiter().GetResult();

        // Then
        Assert.Throws<UnauthorizedException>(action);
    }
}
