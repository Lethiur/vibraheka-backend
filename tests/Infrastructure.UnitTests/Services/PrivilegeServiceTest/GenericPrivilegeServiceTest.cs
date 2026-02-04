using Microsoft.Extensions.Logging;
using Moq;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure.UnitTests.Services.PrivilegeServiceTest;

public abstract class GenericPrivilegeServiceTest
{
    protected Mock<IUserRepository> _userRepositoryMock;
    protected Mock<IActionLogRepository> _actionLogRepositoryMock;
    protected Mock<ILogger<IPrivilegeService>> _loggerMock;
    
    protected PrivilegeService _service;

    [SetUp]
    public void SetUp()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<IPrivilegeService>>();
        _actionLogRepositoryMock = new Mock<IActionLogRepository>();
        _service = new PrivilegeService(_userRepositoryMock.Object, _actionLogRepositoryMock.Object, _loggerMock.Object);
    }
}
