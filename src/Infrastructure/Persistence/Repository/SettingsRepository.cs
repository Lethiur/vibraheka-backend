using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Core.Exceptions;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Common.Interfaces.Settings;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Infrastructure.Persistence.Repository;

public class SettingsRepository(
    IAmazonSimpleSystemsManagement ssmClient,
    AWSConfig config,
    ILogger<SettingsRepository> logger) : ISettingsRepository
{
    /// <summary>
    /// Updates the verification email template in the AWS Systems Manager Parameter Store.
    /// </summary>
    /// <param name="emailTemplate">The email template to be stored as a parameter in AWS Systems Manager.</param>
    /// <param name="cancellationToken">The cancellation token to listen for cancellations.</param>
    /// <returns>A <see cref="Result{T}"/> indicating the success or failure of the operation.</returns>
    public async Task<Result<Unit>> UpdateVerificationEmailTemplateAsync(string emailTemplate,
        CancellationToken cancellationToken)
    {
        string? traceId = GetTraceIdSafe();
        using IDisposable? _ = logger.BeginScope(new Dictionary<string, object?>
            { ["TraceId"] = traceId });

        try
        {
            await ssmClient.PutParameterAsync(
                new PutParameterRequest
                {
                    Name = $"/{config.SettingsNameSpace}/VerificationEmailTemplate",
                    Value = emailTemplate,
                    Type = ParameterType.String,
                    Overwrite = true
                }, cancellationToken);

            return Result.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while updating parameter {ParameterName}", "VerificationEmailTemplate");
            return MapSsmException<Unit>(ex);
        }
    }

    /// <summary>
    /// Updates the password changed email template in the AWS Systems Manager Parameter Store.
    /// </summary>
    /// <param name="emailTemplate">The email template to be stored as a parameter in AWS Systems Manager.</param>
    /// <param name="cancellationToken">The cancellation token to listen for cancellations.</param>
    /// <returns>A <see cref="Result{T}"/> indicating the success or failure of the operation.</returns>
    public async Task<Result<Unit>> UpdatePasswordChangedTemplateAsync(string emailTemplate,
        CancellationToken cancellationToken)
    {
        string? traceId = GetTraceIdSafe();
        using IDisposable? _ = logger.BeginScope(new Dictionary<string, object?>
            { ["TraceId"] = traceId });

        try
        {
            await ssmClient.PutParameterAsync(
                new PutParameterRequest
                {
                    Name = $"/{config.SettingsNameSpace}/PasswordChangedTemplate",
                    Value = emailTemplate,
                    Type = ParameterType.String,
                    Overwrite = true
                }, cancellationToken);

            return Result.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while updating parameter {ParameterName}", "PasswordChangedTemplate");
            return MapSsmException<Unit>(ex);
        }
    }

    /// <summary>
    /// Retrieves the verification email template stored in the AWS Systems Manager Parameter Store.
    /// </summary>
    /// <returns>A <see cref="Result{T}"/> containing the template value if successful, or an infrastructure error code if it fails.</returns>
    public async Task<Result<string>> GetVerificationEmailTemplateAsync()
    {
        string? traceId = GetTraceIdSafe();
        using IDisposable? _ = logger.BeginScope(new Dictionary<string, object?>
            { ["TraceId"] = traceId });

        try
        {
            GetParameterResponse response = await ssmClient.GetParameterAsync(new GetParameterRequest
            {
                Name = $"/{config.SettingsNameSpace}/VerificationEmailTemplate",
                WithDecryption = true
            });

            return Result.Success(response.Parameter.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while getting parameter {ParameterName}", "VerificationEmailTemplate");
            return MapSsmException<string>(ex);
        }
    }

    /// <summary>
    /// Retrieves the password changed email template stored in the AWS Systems Manager Parameter Store.
    /// </summary>
    /// <returns>A <see cref="Result{T}"/> containing the template value if successful, or an infrastructure error code if it fails.</returns>
    public async Task<Result<string>> GetPasswordChangedTemplateAsync()
    {
        string? traceId = GetTraceIdSafe();
        using IDisposable? _ = logger.BeginScope(new Dictionary<string, object?>
            { ["TraceId"] = traceId });

        try
        {
            GetParameterResponse response = await ssmClient.GetParameterAsync(new GetParameterRequest
            {
                Name = $"/{config.SettingsNameSpace}/PasswordChangedTemplate",
                WithDecryption = true
            });

            return Result.Success(response.Parameter.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while getting parameter {ParameterName}", "PasswordChangedTemplate");
            return MapSsmException<string>(ex);
        }
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
            ParameterLimitExceededException => Result.Failure<T>(InfrastructureConfigErrors.ParameterLimitExceeded),
            TooManyUpdatesException => Result.Failure<T>(InfrastructureConfigErrors.TooManyUpdates),
            ParameterNotFoundException => Result.Failure<T>(InfrastructureConfigErrors.ParameterNotFound),
            AmazonSimpleSystemsManagementException => Result.Failure<T>(InfrastructureConfigErrors.AccessDenied),
            _ => Result.Failure<T>(AppErrors.GenericError)
        };
    }

    /// <summary>
    /// Gets the current X-Ray trace ID without throwing when no entity is available in the current context.
    /// </summary>
    /// <returns>The current trace ID if present; otherwise, <c>null</c>.</returns>
    private static string? GetTraceIdSafe()
    {
        try
        {
            return AWSXRayRecorder.Instance.GetEntity()?.Id;
        }
        catch (EntityNotAvailableException)
        {
            return null;
        }
    }
}
