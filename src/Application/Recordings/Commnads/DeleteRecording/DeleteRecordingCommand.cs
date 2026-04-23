using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Interfaces;

namespace VibraHeka.Application.Recordings.Commnads.DeleteRecording;

public record DeleteRecordingCommand(string RecordingId) : IRequest<Result<Unit>>, IRequireAdmin;

