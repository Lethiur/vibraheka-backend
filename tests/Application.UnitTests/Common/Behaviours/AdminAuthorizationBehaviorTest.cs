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
        MockSequence sequence = new();
        bool nextCalled = false;
        Mock<ICurrentUserService> currentUserServiceMock = new();
        Mock<IPrivilegeService> privilegeServiceMock = new();
        currentUserServiceMock.InSequence(sequence).SetupGet(x => x.UserId).Returns("admin-id");
        privilegeServiceMock.InSequence(sequence)
            .Setup(x => x.HasRoleAsync("admin-id", UserRole.Admin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(true));

        AdminAuthorizationBehavior<TestAdminRequest, string> behaviour =
            new(currentUserServiceMock.Object, privilegeServiceMock.Object);

        // When
        string result = await behaviour.Handle(new TestAdminRequest(), _ =>
        {
            nextCalled = true;
            return Task.FromResult("ok");
        }, CancellationToken.None);

        // Then
        Assert.That(result, Is.EqualTo("ok"));
        Assert.That(nextCalled, Is.True);
        currentUserServiceMock.VerifyGet(x => x.UserId, Times.Once);
        privilegeServiceMock.Verify(x => x.HasRoleAsync("admin-id", UserRole.Admin, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public void ShouldThrowUnauthorizedExceptionWhenCurrentUserIsNotAdmin()
    {
        // Given
        MockSequence sequence = new();
        bool nextCalled = false;
        Mock<ICurrentUserService> currentUserServiceMock = new();
        Mock<IPrivilegeService> privilegeServiceMock = new();
        currentUserServiceMock.InSequence(sequence).SetupGet(x => x.UserId).Returns("user-id");
        privilegeServiceMock.InSequence(sequence)
            .Setup(x => x.HasRoleAsync("user-id", UserRole.Admin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(false));

        AdminAuthorizationBehavior<TestAdminRequest, string> behaviour =
            new(currentUserServiceMock.Object, privilegeServiceMock.Object);

        // When
        TestDelegate action = () => behaviour.Handle(new TestAdminRequest(), _ =>
        {
            nextCalled = true;
            return Task.FromResult("ok");
        }, CancellationToken.None).GetAwaiter().GetResult();

        // Then
        Assert.Throws<UnauthorizedException>(action);
        Assert.That(nextCalled, Is.False);
        currentUserServiceMock.VerifyGet(x => x.UserId, Times.Once);
        privilegeServiceMock.Verify(x => x.HasRoleAsync("user-id", UserRole.Admin, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
