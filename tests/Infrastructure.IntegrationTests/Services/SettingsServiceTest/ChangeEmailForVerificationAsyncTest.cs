using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.SettingsServiceTest;

[TestFixture]
public class ChangeEmailForVerificationAsyncTest : GenericSettingsServiceTest
{
    [Test]
    public async Task ShouldUpdateVerificationTemplateSuccessfully()
    {
        string newTemplate = $"verification-template-{_faker.Random.Guid()}";

        Result<Unit> result = await _service.ChangeEmailForVerificationAsync(newTemplate, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
    }

    [TestCase("")]
    [TestCase("   ")]
    [TestCase(null!)]
    public async Task ShouldReturnFailureWhenVerificationTemplateIsInvalid(string invalidTemplate)
    {
        Result<Unit> result = await _service.ChangeEmailForVerificationAsync(invalidTemplate, CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(SettingsErrors.InvalidVerificationEmailTemplate));
    }

    [Test]
    public async Task ShouldUpdateRecoverPasswordEmailTemplateSuccessfully()
    {
        string newTemplate = $"password-changed-template-{_faker.Random.Guid()}";

        Result<Unit> result = await _service.ChangeRecoverPasswordEmailTemplateAsync(newTemplate, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
    }

    [TestCase("")]
    [TestCase("   ")]
    [TestCase(null!)]
    public async Task ShouldReturnFailureWhenRecoverPasswordEmailTemplateIsInvalid(string invalidTemplate)
    {
        Result<Unit> result = await _service.ChangeRecoverPasswordEmailTemplateAsync(invalidTemplate, CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(SettingsErrors.InvalidRecoverPasswordEmailTemplate));
    }
}
