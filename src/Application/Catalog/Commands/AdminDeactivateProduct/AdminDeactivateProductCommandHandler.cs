using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using VibraHeka.Application.Catalog.Commands.AdminActivateProduct;
using VibraHeka.Domain.Catalog.Enums;
using VibraHeka.Domain.Catalog.Errors;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Events.Ports.Out;
using VibraHeka.Domain.Recordings.Ports.Out;

namespace VibraHeka.Application.Catalog.Commands.AdminDeactivateProduct;

public class AdminDeactivateProductCommandHandler(
    IRecordingRegistryPort registryPort,
    IEventRepositoryPort eventRepository,
    ILogger<AdminDeactivateProductCommandHandler> logger) : IRequestHandler<AdminDeActivateProductCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(AdminDeActivateProductCommand request, CancellationToken cancellationToken)
    {
        switch (request.Type)
        {
            case ProductType.DigitalRecording:
                return await registryPort.DeactivateRecordingAsync(request.ProductID, cancellationToken);
            case ProductType.Event:
                return await eventRepository.DeactivateEventAsync(request.ProductID, cancellationToken);
            case ProductType.Therapy:
            case ProductType.SubscriptionPlan:
            default:
                logger.LogWarning("Attempted to de activate product with invalid type: {ProductType}", request.Type);
                return Result.Failure<Unit>(CatalogErrors.InvalidProductType);
        }
    }
}
