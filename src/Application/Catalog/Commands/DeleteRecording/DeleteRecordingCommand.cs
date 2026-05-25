using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Interfaces;

namespace VibraHeka.Application.Recordings.Commnands.DeleteRecording;

public record DeleteRecordingCommand(string RecordingId) : IRequest<Result<Unit>>, IRequireAdmin;

