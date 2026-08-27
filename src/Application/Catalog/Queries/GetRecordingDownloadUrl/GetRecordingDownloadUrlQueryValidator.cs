using VibraHeka.Domain.Catalog.Errors;

namespace VibraHeka.Application.Catalog.Queries.GetRecordingDownloadUrl;

public sealed class GetRecordingDownloadUrlQueryValidator
    : AbstractValidator<GetRecordingDownloadUrlQuery>
{
    public GetRecordingDownloadUrlQueryValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.RecordingId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(RecordingErrors.InvalidRecordingId)
            .Must(id => Guid.TryParse(id, out _))
            .WithMessage(RecordingErrors.InvalidRecordingId);
    }
}

