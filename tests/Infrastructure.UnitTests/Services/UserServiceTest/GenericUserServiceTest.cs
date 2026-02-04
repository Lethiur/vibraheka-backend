using Amazon.CognitoIdentityProvider;
using Microsoft.Extensions.Logging;
using Moq;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure.UnitTests.Services.UserServiceTest;

public abstract class GenericUserServiceTest
{
    protected AWSConfig _configMock;
    protected Mock<ILogger<UserService>> _loggerMock;
    protected Mock<IAmazonCognitoIdentityProvider> _cognitoMock;
    protected TestableUserService _service;
    
    [SetUp]
    public void SetUp()
    {
        _configMock = new AWSConfig();
        _loggerMock = new Mock<ILogger<UserService>>();
        _cognitoMock = new Mock<IAmazonCognitoIdentityProvider>();

        _configMock.Location = "eu-west-1";
        _configMock.Profile = "Twingers";
        
        // Asumiendo que UserService permite inyectar o acceder al cliente para tests
        _service = new TestableUserService(_configMock, _loggerMock.Object, _cognitoMock.Object);
    }
}
