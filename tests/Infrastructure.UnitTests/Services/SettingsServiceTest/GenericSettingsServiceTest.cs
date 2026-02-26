using Microsoft.Extensions.Logging;
using Moq;
using VibraHeka.Domain.Common.Interfaces.Settings;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure.UnitTests.Services.SettingsServiceTest;

public abstract class GenericSettingsServiceTest
{
    protected Mock<ISettingsRepository> RepositoryMock;
    protected SettingsService Service;
    protected Mock<ILogger<SettingsService>> LoggerMock;
    protected AppSettingsEntity AppSettings;
    [SetUp]
    public void SetUp()
    {
        RepositoryMock = new Mock<ISettingsRepository>();
        LoggerMock = new Mock<ILogger<SettingsService>>();
        AppSettings = new AppSettingsEntity();
        Service = new SettingsService(RepositoryMock.Object, AppSettings, LoggerMock.Object);
    }
}
