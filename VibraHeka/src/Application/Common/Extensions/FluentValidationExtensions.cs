using System.Text.RegularExpressions;

namespace VibraHeka.Application.Common.Extensions;

public static class FluentValidationExtensions
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+)*@[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?)*\.[a-zA-Z]{2,}$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    );

    public static IRuleBuilderOptions<T, string> ValidEmail<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.Must(BeValidEmail);
    }

    private static bool BeValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            return EmailRegex.IsMatch(email.Trim());
        }
        catch
        {
            return false;
        }
    }
}
