using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using VibraHeka.Domain.Catalog.Enums;
using VibraHeka.Domain.Catalog.Errors;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Events.Ports.Out;
using VibraHeka.Domain.Recordings.Ports.Out;

namespace VibraHeka.Application.Catalog.Commands.AdminActivateProduct;

public class AdminActivateProductCommandHandler(
    IRecordingRegistryPort registryPort,
    IEventRepositoryPort eventRepository,
    ILogger<AdminActivateProductCommandHandler> logger) : IRequestHandler<AdminActivateProductCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(AdminActivateProductCommand request, CancellationToken cancellationToken)
    {
        switch (request.Type)
        {
            case ProductType.DigitalRecording:
                return await registryPort.ActivateRecordingAsync(request.ProductID, cancellationToken);
            case ProductType.Event:
                return await eventRepository.ActivateEventAsync(request.ProductID, cancellationToken);
            case ProductType.Therapy:
            case ProductType.SubscriptionPlan:
            default:
                logger.LogWarning("Attempted to activate product with invalid type: {ProductType}", request.Type);
                return Result.Failure<Unit>(CatalogErrors.InvalidProductType);
        }
    }
}
