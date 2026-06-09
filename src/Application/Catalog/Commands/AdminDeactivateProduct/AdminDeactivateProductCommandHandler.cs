using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using VibraHeka.Domain.Catalog.Enums;
using VibraHeka.Domain.Catalog.Errors;
using VibraHeka.Domain.Catalog.Ports.Out;
using VibraHeka.Domain.Events.Ports.Out;
using VibraHeka.Domain.Recordings.Ports.Out;

namespace VibraHeka.Application.Catalog.Commands.AdminDeactivateProduct;

public class AdminDeactivateProductCommandHandler(
    IRecordingRegistryPort registryPort,
    IEventRepositoryPort eventRepository,
    ISubscriptionPlanPort subscriptionPlanPort,
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
            case ProductType.SubscriptionPlan:
                return await subscriptionPlanPort.ActivateSubscriptionPlanAsync(request.ProductID, cancellationToken);
            case ProductType.Therapy:
            default:
                logger.LogWarning("Attempted to de activate product with invalid type: {ProductType}", request.Type);
                return Result.Failure<Unit>(CatalogErrors.InvalidProductType);
        }
    }
}
