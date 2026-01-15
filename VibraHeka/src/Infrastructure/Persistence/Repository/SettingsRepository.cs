using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Common.Interfaces.Settings;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Infrastructure.Persistence.Repository;

public class SettingsRepository(IAmazonSimpleSystemsManagement SsmClient) : ISettingsRepository
{
    /// <summary>
    /// Updates the verification email template in the AWS Systems Manager Parameter Store.
    /// </summary>
    /// <param name="emailTemplate">The email template to be stored as a parameter in AWS Systems Manager.</param>
    /// <param name="cancellationToken">The cancellation token to listen for cancellations</param>
    /// <returns>A <see cref="Result{T}"/> indicating the success or failure of the operation. If successful, returns <see cref="Unit.Value"/>.</returns>
    public async Task<Result<Unit>> UpdateVerificationEmailTemplateAsync(string emailTemplate, CancellationToken cancellationToken)
    {
        try
        {
            await SsmClient.PutParameterAsync(new PutParameterRequest
            {
                Name = "/VibraHeka/VerificationEmailTemplate",
                Value = emailTemplate,
                Type = ParameterType.String,
                Overwrite = true
            }, cancellationToken);
            
            return Result.Success<Unit>(Unit.Value);
        }
        catch (ParameterLimitExceededException)
        {
            return Result.Failure<Unit>(InfrastructureConfigErrors.ParameterLimitExceeded);
        }
        catch (TooManyUpdatesException)
        {
            return Result.Failure<Unit>(InfrastructureConfigErrors.TooManyUpdates);
        }
        catch (AmazonSimpleSystemsManagementException)
        {
            return Result.Failure<Unit>(InfrastructureConfigErrors.AccessDenied);
        }
        catch (Exception)
        {
            return Result.Failure<Unit>(InfrastructureConfigErrors.UnexpectedError);
        }
    }

    public Task<Result<string>> GetVerificationEmailTemplateAsync()
    {
        throw new NotImplementedException();
    }
}
