using VibraHeka.Domain.Catalog.Errors;

namespace VibraHeka.Application.Catalog.Commands.DeleteRecording;

public class DeleteRecordingCommandValidator : AbstractValidator<DeleteRecordingCommand>
{
    public DeleteRecordingCommandValidator()
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

