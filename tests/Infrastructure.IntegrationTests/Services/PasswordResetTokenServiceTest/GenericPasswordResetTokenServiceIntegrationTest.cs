using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.PasswordResetTokenServiceTest;

public abstract class GenericPasswordResetTokenServiceIntegrationTest : TestBase
{
    protected IPasswordResetTokenService _service;

    [OneTimeSetUp]
    public void OneTimeSetUpChild()
    {
        base.OneTimeSetUp();
        _service = new PasswordResetTokenService(_configuration, CreateTestLogger<PasswordResetTokenService>());
    }
}
