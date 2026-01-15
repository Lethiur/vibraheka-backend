using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Common.Interfaces.Settings;

namespace VibraHeka.Infrastructure.Services;

/// <summary>
/// Service class responsible for managing application settings, including
/// operations related to the verification email template.
/// </summary>
public class SettingsService(ISettingsRepository Repository) : ISettingsService
{
    /// <summary>
    /// Updates the email template used for verification purposes.
    /// Validates the provided email string and updates the template in the repository if valid.
    /// </summary>
    /// <param name="email">The new email template to be used for verification.</param>
    /// <returns>A <see cref="Result{Unit}"/> indicating the success or failure of the operation,
    /// along with potential error details.</returns>
    public async Task<Result<Unit>> ChangeEmailForVerificationAsync(string email, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure<Unit>(SettingsErrors.InvalidVerificationEmailTemplate);
        }
        
        return await Repository.UpdateVerificationEmailTemplateAsync(email, cancellationToken);
    }
}
