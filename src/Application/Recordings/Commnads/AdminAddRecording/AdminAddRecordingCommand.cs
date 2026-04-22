using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Interfaces;
using VibraHeka.Application.Recordings.Entities;
using VibraHeka.Domain.Recordings.Enums;

namespace VibraHeka.Application.Recordings.Commnads.AdminAddRecording;

public sealed record AdminAddRecordingCommand(
    string Name,
    string Description,
    RecordingType Type,
    string FileName) : IRequest<Result<AddRecordingResult>>, IRequireAdmin;
