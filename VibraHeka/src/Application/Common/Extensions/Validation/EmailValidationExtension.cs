using System.Text.RegularExpressions;
using VibraHeka.Application.Common.Exceptions;

namespace VibraHeka.Application.Common.Extensions.Validation;

/// <summary>
/// Provides an extension method for validating email addresses.
/// </summary>
public static class EmailValidationExtension
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+)*@[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?)*\.[a-zA-Z]{2,}$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    );

    /// <summary>
    /// Validates that a string represents a valid email address.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder to which the validation rule is appended.</param>
    /// <returns>An instance of <c>IRuleBuilderOptions</c> that indicates the validation rule for a valid email address.</returns>
    public static IRuleBuilderOptions<T, string> ValidEmail<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.NotEmpty().WithMessage(UserErrors.InvalidEmail).NotNull()
            .WithMessage(UserErrors.InvalidEmail).Must(BeValidEmail)
            .WithMessage(UserErrors.InvalidEmail).Must(BeValidEmail);
    }

    private static bool BeValidEmail(string email)
    {
       
        bool doesMatch = EmailRegex.IsMatch(email.Trim());
        if (doesMatch)
        {
            return email.Trim().Length < 255;
        }

        return doesMatch;
    }
}
