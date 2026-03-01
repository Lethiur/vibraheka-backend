using Microsoft.Extensions.Logging;
using Moq;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure.UnitTests.Services.UserCodeServiceTest;

public abstract class GenericUserCodeServiceTest
{
    protected Mock<IUserCodeRepository> UserCodeRepositoryMock;
    protected Mock<ILogger<UserCodeService>> LoggerMock;
    protected UserCodeService Service;

    [SetUp]
    public void SetUp()
    {
        UserCodeRepositoryMock = new Mock<IUserCodeRepository>();
        LoggerMock = new Mock<ILogger<UserCodeService>>();
        Service = new UserCodeService(UserCodeRepositoryMock.Object, LoggerMock.Object);
    }
}
