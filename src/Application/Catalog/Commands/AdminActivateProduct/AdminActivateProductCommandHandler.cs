using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using VibraHeka.Domain.Catalog.Enums;
using VibraHeka.Domain.Catalog.Errors;
using VibraHeka.Domain.Catalog.Ports.Out;
using VibraHeka.Domain.Events.Ports.Out;

namespace VibraHeka.Application.Catalog.Commands.AdminActivateProduct;

public class AdminActivateProductCommandHandler(
    IRecordingRegistryPort registryPort,
    IEventRepositoryPort eventRepository,
    ISubscriptionPlanPort subscriptionPlanPort,
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
            case ProductType.SubscriptionPlan:
                return await subscriptionPlanPort.ActivateSubscriptionPlanAsync(request.ProductID, cancellationToken);
            case ProductType.Therapy:
                
            default:
                logger.LogWarning("Attempted to activate product with invalid type: {ProductType}", request.Type);
                return Result.Failure<Unit>(CatalogErrors.InvalidProductType);
        }
    }
}
