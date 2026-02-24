using System.ComponentModel;
using System.Data;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Logging.Abstractions;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Infrastructure.Persistence.Repository;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.UserServiceTest;

public class CreateClientTest : TestBase
{
    private NullLogger<UserService> _loggerMock;
    private IUserRepository _userRepositoryMock;
    private IDynamoDBContext _context;


    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        base.OneTimeSetUp();
        _context = CreateDynamoDBContext();
        _userRepositoryMock = new UserRepository(_context, _configuration);
        _loggerMock = new NullLogger<UserService>();
        _userRepositoryMock = new UserRepository(CreateDynamoDBContext(), _configuration);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _context?.Dispose();
    }

    [SetUp]
    public void SetUp()
    {
    }

    [Test]
    [DisplayName("Should throw DataException when AWS profile is missing in configuration")]
    public void ShouldThrowDataExceptionWhenProfileIsMissing()
    {
        _configuration.Profile = null!;
        // When/Then: Instantiating the service should trigger CreateClient and throw
        DataException? ex =
            Assert.Throws<DataException>(() => new UserService(_configuration, _loggerMock, _userRepositoryMock));
        Assert.That(ex.Message, Is.EqualTo("AWS profile is required"));
    }

    [Test]
    [DisplayName("Should throw DataException when AWS profile exists in config but not in system")]
    public void ShouldThrowDataExceptionWhenProfileNotFoundInSystem()
    {
        // Given: A profile name that definitely doesn't exist on the machine
        _configuration.Profile = $"non-existent-profile-{Guid.NewGuid()}";

        // When/Then: It should fail because CredentialProfileStoreChain won't find it

        DataException? ex =
            Assert.Throws<DataException>(() => new UserService(_configuration, _loggerMock, _userRepositoryMock));

        Assert.That(ex.Message, Is.EqualTo("AWS profile is required"));
    }

    [Test]
    [DisplayName("Should initialize successfully when a valid local profile is provided")]
    [NUnit.Framework.Category("LocalOnly")]
    public void ShouldInitializeSuccessfullyWithValidProfile()
    {
        // Given: A valid profile name (ajusta "default" a uno que tengas o usa uno de pruebas)
        _configuration.Profile = "Twingers";

        // When: Creating the service
        // Then: Should not throw if the profile exists in ~/.aws/credentials
        Assert.DoesNotThrow(() => new UserService(_configuration, _loggerMock, _userRepositoryMock));
    }
}
