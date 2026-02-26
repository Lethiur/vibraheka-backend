using CSharpFunctionalExtensions;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.SettingsServiceTest;

[TestFixture]
public class GetTemplatesForActionsTest : GenericSettingsServiceTest
{
    [Test]
    public void ShouldReturnCurrentStateOfTemplatesFromMemory()
    {
        const string verificationTemplate = "verification-memory-value";
        const string passwordChangedTemplate = "password-changed-memory-value";
        _appSettings.VerificationEmailTemplate = verificationTemplate;
        _appSettings.PasswordChangedTemplate = passwordChangedTemplate;

        Result<IEnumerable<TemplateForActionEntity>> result = _service.GetAllTemplatesForActions();

        Assert.That(result.IsSuccess, Is.True);
        List<TemplateForActionEntity> templates = result.Value.ToList();
        Assert.That(templates, Has.Count.EqualTo(2));
        Assert.That(templates[0].TemplateID, Is.EqualTo(verificationTemplate));
        Assert.That(templates[1].TemplateID, Is.EqualTo(passwordChangedTemplate));
    }
}
