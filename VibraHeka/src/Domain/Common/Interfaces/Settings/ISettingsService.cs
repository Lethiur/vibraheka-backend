using CSharpFunctionalExtensions;
using MediatR;

namespace VibraHeka.Domain.Common.Interfaces.Settings;

/// <summary>
/// Defines the contract for a service that manages application settings and
/// provides functionality related to email verification templates.
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Updates the email template used for verification purposes.
    /// Validates that the provided email template is not null or whitespace,
    /// and updates the template in the repository if valid.
    /// </summary>
    /// <param name="emailTemplate">The new email template to be used for verification.</param>
    /// <returns>A <c>Result{Unit}</c> indicating the success or failure of the operation,
    /// including potential error details.</returns>
    Task<Result<Unit>> ChangeEmailForVerificationAsync(string emailTemplate, CancellationToken cancellationToken);
}
