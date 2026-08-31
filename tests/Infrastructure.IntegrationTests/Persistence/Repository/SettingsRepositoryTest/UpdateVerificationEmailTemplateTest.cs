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
        Guid emailTemplate = Guid.NewGuid();

        // When: Updating the template in SSM
        Result<Unit> result = await Repository.UpdateVerificationEmailTemplateAsync(emailTemplate, CancellationToken.None);

        // Then: Should return success
        Assert.That(result.IsSuccess, Is.True);

        // And: Verify the value was actually stored in AWS SSM
        GetParameterResponse? response = await SSMClient.GetParameterAsync(new GetParameterRequest { Name = VerificationParameterName });
        Assert.That(response.Parameter.Value, Is.EqualTo(emailTemplate.ToString()));
    }

    [Test]
    [DisplayName("Should overwrite existing template when updated again")]
    public async Task ShouldOverwriteExistingTemplate()
    {
        // Given: An initial template already in SSM
        await Repository.UpdateVerificationEmailTemplateAsync(Guid.NewGuid(), CancellationToken.None);
        Guid newTemplate = Guid.NewGuid();

        // When: Updating the same parameter
        Result<Unit> result = await Repository.UpdateVerificationEmailTemplateAsync(newTemplate, CancellationToken.None);

        // Then: Should succeed and reflect the new value
        Assert.That(result.IsSuccess, Is.True);

        GetParameterResponse? response = await SSMClient.GetParameterAsync(new GetParameterRequest { Name = VerificationParameterName });
        Assert.That(response.Parameter.Value, Is.EqualTo(newTemplate.ToString()));
    }

    #endregion

    #region UpdateVerificationEmailTemplate - Edge Cases


    [Test]
    [DisplayName("Should return generic error when operation is cancelled")]
    public async Task ShouldReturnGenericErrorWhenCancellationIsRequested()
    {
        // Given: un token de cancelacion ya cancelado.
        using CancellationTokenSource cts = new();
        await cts.CancelAsync();

        // When: se intenta actualizar con la operacion cancelada.
        Result<Unit> result = await Repository.UpdateVerificationEmailTemplateAsync(Guid.NewGuid(), cts.Token);

        // Then: el repositorio debe mapear al error generico de aplicacion.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(VibraHeka.Application.Common.Exceptions.AppErrors.GenericError));
    }

    #endregion
}
