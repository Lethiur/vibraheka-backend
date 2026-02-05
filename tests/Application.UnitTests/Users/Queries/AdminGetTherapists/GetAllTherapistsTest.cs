using System.ComponentModel;
using CSharpFunctionalExtensions;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Admin.Queries.GetAllTherapists;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Application.UnitTests.Users.Queries.AdminGetTherapists;

[TestFixture]
public class GetAllTherapistsQueryHandlerTests
{
    private Mock<IUserRepository> RepositoryMock;
    private GetAllTherapistsQueryHandler Handler;

    [SetUp]
    public void SetUp()
    {
        RepositoryMock = new Mock<IUserRepository>();
        
        Handler = new GetAllTherapistsQueryHandler(
            RepositoryMock.Object);
    }

    [Test]
    [DisplayName("Should return a list of therapists when user is admin")]
    public async Task ShouldReturnAListOfTherapistsWhenUserIsAdmin()
    {
        // Given: An admin user and some therapists in the repository
        IEnumerable<UserEntity> therapists = new List<UserEntity> { new() { FirstName = "Therapist Name" } };
        GetAllTherapistsQuery query = new();
        
        RepositoryMock.Setup(x => x.GetByRoleAsync(UserRole.Therapist))
            .ReturnsAsync(Result.Success(therapists));

        // When: The handler is executed
        Result<IEnumerable<UserEntity>> result = await Handler.Handle(query, CancellationToken.None);

        // Then: The result should be successful and contain the therapists
        Assert.That(result.IsSuccess, Is.True, "The operation should be successful");
        Assert.That(result.Value, Is.EquivalentTo(therapists), "The returned list should match the therapists in the repository");
        RepositoryMock.Verify(x => x.GetByRoleAsync(UserRole.Therapist), Times.Once);
    }

}
