using VibraHeka.Domain.Recordings.Errors;

namespace VibraHeka.Application.Recordings.Commnads.AdminAddRecording;

public class AdminAddRecordingCommandValidator : AbstractValidator<AdminAddRecordingCommand>
{
    public AdminAddRecordingCommandValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(RecordingErrors.InvalidName)
            .MaximumLength(200)
            .WithMessage(RecordingErrors.InvalidName);

        RuleFor(x => x.Description)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(RecordingErrors.InvalidDescription)
            .MaximumLength(2000)
            .WithMessage(RecordingErrors.InvalidDescription);

        RuleFor(x => x.Type)
            .Cascade(CascadeMode.Stop)
            .IsInEnum()
            .WithMessage(RecordingErrors.InvalidType);

        RuleFor(x => x.FileName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(RecordingErrors.InvalidFile)
            .Matches(@"^[^/\\:*?""<>|]+\.[a-zA-Z0-9]+$")
            .WithMessage(RecordingErrors.InvalidFile);
    }
}
