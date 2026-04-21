using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Interfaces;
using VibraHeka.Domain.Recordings.Enums;

namespace VibraHeka.Application.Recordings.Commnads.AdminAddRecording;

public record AdminAddRecordingCommand(
    string Name,
    string Description,
    RecordingType Type,
    Stream FileStream,
    string FileName) : IRequest<Result<string>>, IRequireAdmin;
