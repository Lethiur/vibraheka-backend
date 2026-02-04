using Amazon.CognitoIdentityProvider;
using Microsoft.Extensions.Logging;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure.UnitTests.Services.UserServiceTest;

public class TestableUserService : UserService
{
    public TestableUserService(AWSConfig config, ILogger<UserService> logger, IAmazonCognitoIdentityProvider mockClient) 
        : base(config, logger)
    {
        _client = mockClient;
        // Nota: En un escenario real, deberías usar reflexión o inyección 
        // para sustituir el campo privado _client con MockClient.Object
    }
}
