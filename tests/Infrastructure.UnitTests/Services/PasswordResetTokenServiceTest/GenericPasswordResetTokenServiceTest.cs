using Microsoft.Extensions.Logging;
using Moq;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure.UnitTests.Services.PasswordResetTokenServiceTest;

public abstract class GenericPasswordResetTokenServiceTest
{
    protected AWSConfig Config;
    protected Mock<ILogger<PasswordResetTokenService>> LoggerMock;
    protected PasswordResetTokenService Service;

    [SetUp]
    public void SetUp()
    {
        Config = new AWSConfig
        {
            PasswordResetTokenSecret = "super-secret-for-tests"
        };
        LoggerMock = new Mock<ILogger<PasswordResetTokenService>>();
        Service = new PasswordResetTokenService(Config, LoggerMock.Object);
    }
}
