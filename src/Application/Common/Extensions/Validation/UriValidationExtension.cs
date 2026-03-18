namespace VibraHeka.Application.Common.Extensions.Validation;

public static class UriValidationExtension
{
    public static IRuleBuilderOptions<T, string> ValidURL<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.Must(url => Uri.TryCreate(url, UriKind.Absolute, out Uri? uri)
                                       && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps));
    }
}
