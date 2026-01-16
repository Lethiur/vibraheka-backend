using Amazon.CognitoIdentityProvider;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure.UnitTests.Services.UserServiceTest;

public abstract class GenericUserServiceTest
{
    protected Mock<IConfiguration> _configMock;
    protected Mock<ILogger<UserService>> _loggerMock;
    protected Mock<IAmazonCognitoIdentityProvider> _cognitoMock;
    protected TestableUserService _service;
    
    [SetUp]
    public void SetUp()
    {
        _configMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<UserService>>();
        _cognitoMock = new Mock<IAmazonCognitoIdentityProvider>();

        _configMock.Setup(c => c["AWS:Region"]).Returns("eu-west-1");
        _configMock.Setup(c => c["AWS:Profile"]).Returns("Twingers");
        
        // Asumiendo que UserService permite inyectar o acceder al cliente para tests
        _service = new TestableUserService(_configMock.Object, _loggerMock.Object, _cognitoMock.Object);
    }
}
