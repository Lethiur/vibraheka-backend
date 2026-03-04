using System.ComponentModel;
using Amazon.SimpleSystemsManagement.Model;
using CSharpFunctionalExtensions;
using MediatR;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.SettingsRepositoryTest;

[TestFixture]
public class UpdateVerificationEmailTemplateTest : GenericSettingsRepositoryTest
{

    #region UpdateVerificationEmailTemplate - Success Cases

    [Test]
    [DisplayName("Should update verification email template successfully in SSM")]
    public async Task ShouldUpdateVerificationEmailTemplateSuccessfully()
    {
        // Given: A random email template content
        string emailTemplate = $"<html><body><h1>Verify your email</h1><p>{_faker.Lorem.Sentence()}</p></body></html>";

        // When: Updating the template in SSM
        Result<Unit> result = await Repository.UpdateVerificationEmailTemplateAsync(emailTemplate, CancellationToken.None);

        // Then: Should return success
        Assert.That(result.IsSuccess, Is.True);

        // And: Verify the value was actually stored in AWS SSM
        GetParameterResponse? response = await SSMClient.GetParameterAsync(new GetParameterRequest { Name = VerificationParameterName });
        Assert.That(response.Parameter.Value, Is.EqualTo(emailTemplate));
    }

    [Test]
    [DisplayName("Should overwrite existing template when updated again")]
    public async Task ShouldOverwriteExistingTemplate()
    {
        // Given: An initial template already in SSM
        await Repository.UpdateVerificationEmailTemplateAsync("initial template", CancellationToken.None);
        string newTemplate = "updated template " + _faker.Random.Guid();

        // When: Updating the same parameter
        Result<Unit> result = await Repository.UpdateVerificationEmailTemplateAsync(newTemplate, CancellationToken.None);

        // Then: Should succeed and reflect the new value
        Assert.That(result.IsSuccess, Is.True);
        
        GetParameterResponse? response = await SSMClient.GetParameterAsync(new GetParameterRequest { Name = VerificationParameterName });
        Assert.That(response.Parameter.Value, Is.EqualTo(newTemplate));
    }

    #endregion

    #region UpdateVerificationEmailTemplate - Edge Cases

    [Test]
    [DisplayName("Should handle very large template content")]
    public async Task ShouldHandleLargeTemplateContent()
    {
        // Given: A large string (SSM Standard parameters support up to 4KB)
        string largeTemplate = new('A', 3000); 

        // When: Updating the template
        Result<Unit> result = await Repository.UpdateVerificationEmailTemplateAsync(largeTemplate, CancellationToken.None);

        // Then: Should return success if within limits
        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    [DisplayName("Should return generic error when operation is cancelled")]
    public async Task ShouldReturnGenericErrorWhenCancellationIsRequested()
    {
        // Given: un token de cancelacion ya cancelado.
        using CancellationTokenSource cts = new();
        cts.Cancel();

        // When: se intenta actualizar con la operacion cancelada.
        Result<Unit> result = await Repository.UpdateVerificationEmailTemplateAsync("template-cancelled", cts.Token);

        // Then: el repositorio debe mapear al error generico de aplicacion.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(VibraHeka.Application.Common.Exceptions.AppErrors.GenericError));
    }

    #endregion
}
