using VibraHeka.Application.Common.Exceptions;

namespace VibraHeka.Application.Common.Extensions.Validation;

/// <summary>
/// Provides extension methods for password validation using the FluentValidation library.
/// </summary>
public static class PasswordValidationExtension
{
    /// <summary>
    /// Validates that a password meets specific requirements such as not being empty, having a minimum length,
    /// and being non-null. Incorporates predefined error messages for invalid passwords.
    /// </summary>
    /// <typeparam name="T">The type of the validating entity.</typeparam>
    /// <param name="ruleBuilder">The rule builder where the password validation rules are applied.</param>
    /// <returns>An IRuleBuilderOptions object containing the configured validation rules.</returns>
    public static IRuleBuilderOptions<T, string> ValidPassword<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.NotEmpty().WithMessage(UserException.InvalidPassword).NotNull().WithMessage(UserException.InvalidPassword).MinimumLength(6).WithMessage(UserException.InvalidPassword);
    }
}
