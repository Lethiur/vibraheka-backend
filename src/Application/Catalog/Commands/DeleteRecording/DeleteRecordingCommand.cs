using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Interfaces;

namespace VibraHeka.Application.Catalog.Commands.DeleteRecording;

public record DeleteRecordingCommand(string RecordingId) : IRequest<Result<Unit>>, IRequireAdmin;

