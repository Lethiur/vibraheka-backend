using Amazon.CognitoIdentityProvider;
using Microsoft.Extensions.Logging;
using Moq;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure.UnitTests.Services.UserServiceTest;

public abstract class GenericUserServiceTest
{
    protected AWSConfig ConfigMock;
    protected Mock<ILogger<UserService>> LoggerMock;
    protected Mock<IAmazonCognitoIdentityProvider> CognitoMock;
    protected Mock<IUserRepository> _userRepositoryMock;
    protected TestableUserService _service;
    
    [SetUp]
    public void SetUp()
    {
        ConfigMock = new AWSConfig();
        LoggerMock = new Mock<ILogger<UserService>>();
        CognitoMock = new Mock<IAmazonCognitoIdentityProvider>();
        _userRepositoryMock = new Mock<IUserRepository>();

        ConfigMock.Location = "eu-west-1";
        ConfigMock.Profile = "Twingers";
        
        // Asumiendo que UserService permite inyectar o acceder al cliente para tests
        _service = new TestableUserService(ConfigMock, LoggerMock.Object, CognitoMock.Object, _userRepositoryMock.Object);
    }
}
