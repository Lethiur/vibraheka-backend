using System.ComponentModel;
using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.UserServiceTest;

public class CreateClientTest
{
   private Mock<IConfiguration> _configMock;
    private Mock<ILogger<UserService>> _loggerMock;

    [SetUp]
    public void SetUp()
    {
        _configMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<UserService>>();
    }

    [Test]
    [DisplayName("Should throw DataException when AWS profile is missing in configuration")]
    public void ShouldThrowDataExceptionWhenProfileIsMissing()
    {
        // Given: Configuration without AWS:Profile
        _configMock.Setup(c => c["AWS:Profile"]).Returns((string)null!);
        _configMock.Setup(c => c["AWS:Region"]).Returns("eu-west-1");

        // When/Then: Instantiating the service should trigger CreateClient and throw
        DataException? ex = Assert.Throws<DataException>(() => new UserService(_configMock.Object, _loggerMock.Object));
        Assert.That(ex.Message, Is.EqualTo("AWS profile is required"));
    }

    [Test]
    [DisplayName("Should throw DataException when AWS profile exists in config but not in system")]
    public void ShouldThrowDataExceptionWhenProfileNotFoundInSystem()
    {
        // Given: A profile name that definitely doesn't exist on the machine
        string fakeProfile = $"non-existent-profile-{Guid.NewGuid()}";
        _configMock.Setup(c => c["AWS:Profile"]).Returns(fakeProfile);
        _configMock.Setup(c => c["AWS:Region"]).Returns("eu-west-1");

        // When/Then: It should fail because CredentialProfileStoreChain won't find it
        DataException? ex = Assert.Throws<DataException>(() =>
        {
            UserService userService = new UserService(_configMock.Object, _loggerMock.Object);
        });
        Assert.That(ex.Message, Is.EqualTo("AWS profile is required"));
    }

    [Test]
    [DisplayName("Should initialize successfully when a valid local profile is provided")]
    [NUnit.Framework.Category("LocalOnly")] 
    public void ShouldInitializeSuccessfullyWithValidProfile()
    {
        // Given: A valid profile name (ajusta "default" a uno que tengas o usa uno de pruebas)
        const string validProfile = "Twingers"; 
        _configMock.Setup(c => c["AWS:Profile"]).Returns(validProfile);
        _configMock.Setup(c => c["AWS:Region"]).Returns("eu-west-1");

        // When: Creating the service
        // Then: Should not throw if the profile exists in ~/.aws/credentials
        Assert.DoesNotThrow(() => new UserService(_configMock.Object, _loggerMock.Object));
    }
}
