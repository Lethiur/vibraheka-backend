using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.EmailTemplates.Ports.Out;
using VibraHeka.Infrastructure.Entities;

namespace Infrastructure.AWS.SSM;

public class EmailTemplateConfigAdapter(IAmazonSimpleSystemsManagement ssmClient, IOptionsMonitor<AWSConfig> awsConfig, ILogger<EmailTemplateConfigAdapter> logger) : EmailTemplateConfigPort
{
    public async Task<Result<Unit>> ChangeEmailTemplateKeyForAction(string emailTemplateKey, string emailTemplateID,
        CancellationToken cancellationToken)
    {
        string fullName = BuildParameterName(emailTemplateKey);
        try
        {
            await ssmClient.PutParameterAsync(
                new PutParameterRequest
                {
                    Name = fullName,
                    Value = emailTemplateID,
                    Type = ParameterType.String,
                    Overwrite = true
                }, cancellationToken);
            logger.LogInformation("{ParameterName} updated successfully with templateID {TemplateID}", fullName, emailTemplateID);
            return Result.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while updating parameter {ParameterName}", fullName);
            return MapSsmException<Unit>(ex);
        }
    }

    public async Task<Result<string>> GetEmailTemplateKeyForAction(string emailTemplateKey)
    {
        string fullName = BuildParameterName(emailTemplateKey);
        
        try
        {
            GetParameterResponse response = await ssmClient.GetParameterAsync(new GetParameterRequest
            {
                Name = fullName,
                WithDecryption = true
            });

            return Result.Success(response.Parameter.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while getting parameter {ParameterName}", fullName);
            return MapSsmException<string>(ex);
        }
    }
    
    private string BuildParameterName(string parameterName)
    {
        return $"/{awsConfig.CurrentValue.SettingsNameSpace}/{parameterName}";
    }

    /// <summary>
    /// Maps exceptions thrown by AWS SSM to infrastructure-level error codes.
    /// </summary>
    /// <typeparam name="T">The type of result returned by the operation.</typeparam>
    /// <param name="ex">The exception thrown while executing an operation in AWS SSM.</param>
    /// <returns>A <see cref="Result{T}"/> with an infrastructure error code or <see cref="AppErrors.GenericError"/> for unknown exceptions.</returns>
    private static Result<T> MapSsmException<T>(Exception ex)
    {
        return ex switch
        {
            ParameterLimitExceededException => Result.Failure<T>(AppErrors.UnknownError),
            TooManyUpdatesException => Result.Failure<T>(AppErrors.UnknownError),
            ParameterNotFoundException => Result.Failure<T>(AppErrors.UnknownError),
            AmazonSimpleSystemsManagementException => Result.Failure<T>(AppErrors.UnknownError),
            _ => Result.Failure<T>(AppErrors.GenericError)
        };
    }
}
