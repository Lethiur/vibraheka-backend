using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Application.Users.Queries.GetProfile;

public class GetUserProfileQueryValidator : AbstractValidator<GetUserProfileQuery>
{
    public GetUserProfileQueryValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(x => x.UserID)
            .NotNull()
            .WithMessage(ProfileErrors.InvalidProfileID)
            .NotEmpty()
            .WithMessage(ProfileErrors.InvalidProfileID);
    }
}
