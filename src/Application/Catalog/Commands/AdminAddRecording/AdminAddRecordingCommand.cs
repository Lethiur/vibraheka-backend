using CSharpFunctionalExtensions;
using NMoneys;
using VibraHeka.Application.Common.Interfaces;
using VibraHeka.Application.Recordings.Entities;
using VibraHeka.Domain.Recordings.Enums;

namespace VibraHeka.Application.Recordings.Commnands.AdminAddRecording;

public sealed record AdminAddRecordingCommand(
    string Name,
    string Description,
    decimal Price, 
    CurrencyIsoCode CurrencyCode,
    RecordingTier Tier,
    RecordingType Type) : IRequest<Result<AddRecordingResult>>, IRequireAdmin;
