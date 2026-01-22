using CSharpFunctionalExtensions;
using Moq;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Common.Interfaces.Settings;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure.UnitTests.Services.SettingsServiceTest;

[TestFixture]
public class GetAllTemplatesForActionsTest
{
    private Mock<ISettingsRepository> RepositoryMock;
    private AppSettingsEntity AppSettings;
    private SettingsService Service;

    [SetUp]
    public void SetUp()
    {
        RepositoryMock = new Mock<ISettingsRepository>();
        // Creamos una instancia real de AppSettings ya que es una entidad de datos simple
        AppSettings = new AppSettingsEntity();

        Service = new SettingsService(RepositoryMock.Object, AppSettings);
    }

    [Test]
    public void ShouldReturnTemplatesMappingCorrectlyFromAppSettings()
    {
        // Given
        const string verificationTemplate = "verification-id-123";
        const string resetPasswordTemplate = "reset-id-456";

        AppSettings.VerificationEmailTemplate = verificationTemplate;
        AppSettings.EmailForResetPassword = resetPasswordTemplate;

        // When
        Result<IEnumerable<TemplateForActionEntity>> result = Service.GetAllTemplatesForActions();

        // Then
        Assert.That(result.IsSuccess, Is.True);
        List<TemplateForActionEntity> templates = result.Value.ToList();

        Assert.That(templates, Has.Count.EqualTo(2));

        // Validamos el primer template (Verification)
        Assert.That(templates[0].TemplateID, Is.EqualTo(verificationTemplate));

        // Validamos el segundo template (Reset Password)
        Assert.That(templates[1].TemplateID, Is.EqualTo(resetPasswordTemplate));
        Assert.That(templates[1].ActionType, Is.EqualTo(ActionType.PasswordReset));
    }

    [Test]
    public void ShouldReturnEmptyValuesWhenAppSettingsAreEmpty()
    {
        // Given
        AppSettings.VerificationEmailTemplate = string.Empty;
        AppSettings.EmailForResetPassword = string.Empty;

        // When
        Result<IEnumerable<TemplateForActionEntity>> result = Service.GetAllTemplatesForActions();

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.All(t => string.IsNullOrEmpty(t.TemplateID)), Is.True);
    }
}
