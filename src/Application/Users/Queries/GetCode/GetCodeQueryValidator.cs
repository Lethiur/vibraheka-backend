using VibraHeka.Application.Common.Extensions.Validation;

namespace VibraHeka.Application.Users.Queries.GetCode;

public class GetCodeQueryValidator : AbstractValidator<GetCodeQuery>
{
    public GetCodeQueryValidator()
    {
        RuleFor(x => x.UserName)
            .Cascade(CascadeMode.Stop)
            .ValidEmail();
    }
}
