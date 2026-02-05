using CSharpFunctionalExtensions;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Admin.Queries.GetAllTherapists;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Application.FunctionalTests.Users;

[TestFixture]
public class GetAllTherapistsQueryHandlerTest
{
    private Mock<IUserRepository> _repoMock = default!;
    private GetAllTherapistsQueryHandler _handler = default!;

    [SetUp]
    public void SetUp()
    {
        _repoMock = new Mock<IUserRepository>();
        _handler = new GetAllTherapistsQueryHandler(_repoMock.Object);
    }

    [Test]
    [Description("Given a request for all therapists, when the repository returns a successful list, then the handler should return that list")]
    public async Task ShouldReturnTherapistsWhenRepositorySucceeds()
    {
        // Given
        IEnumerable<UserEntity> therapists = [new UserEntity { Role = UserRole.Therapist, Email = "t1@test.com" }];
        _repoMock.Setup(x => x.GetByRoleAsync(UserRole.Therapist)).ReturnsAsync(Result.Success(therapists));

        // When
        Result<IEnumerable<UserEntity>> result = await _handler.Handle(new GetAllTherapistsQuery(), CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(therapists));
    }

    [Test]
    [Description("Given a request for all therapists, when the repository returns a failure result, then the handler should return that failure")]
    public async Task ShouldReturnFailureWhenRepositoryFails()
    {
        // Given
        string errorMessage = "DB-FAIL";
        _repoMock.Setup(x => x.GetByRoleAsync(UserRole.Therapist))
            .ReturnsAsync(Result.Failure<IEnumerable<UserEntity>>(errorMessage));

        // When
        Result<IEnumerable<UserEntity>> result = await _handler.Handle(new GetAllTherapistsQuery(), CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(errorMessage));
    }
}

