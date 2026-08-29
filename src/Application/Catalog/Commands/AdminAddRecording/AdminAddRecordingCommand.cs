using CSharpFunctionalExtensions;
using NMoneys;
using VibraHeka.Application.Catalog.Models;
using VibraHeka.Application.Common.Interfaces;
using VibraHeka.Domain.Catalog.Enums;

namespace VibraHeka.Application.Catalog.Commands.AdminAddRecording;

public sealed record AdminAddRecordingCommand(
    string Name,
    string Description,
    decimal Price,
    CurrencyIsoCode CurrencyCode,
    RecordingTier Tier,
    RecordingType Type) : IRequest<Result<AddRecordingResult>>, IRequireAdmin;
