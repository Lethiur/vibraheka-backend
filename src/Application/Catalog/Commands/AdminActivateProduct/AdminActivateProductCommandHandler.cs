using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Events.Ports.Out;
using VibraHeka.Domain.Recordings.Ports.Out;

namespace VibraHeka.Application.Catalog.Commands.AdminActivateProduct;

public class AdminActivateProductCommandHandler(
    ICurrentUserService currentUserService,
    IRecordingRegistryPort registryPort,
    IEventRepositoryPort eventRepository) : IRequestHandler<AdminActivateProductCommand, Result<Unit>>
{
    public Task<Result<Unit>> Handle(AdminActivateProductCommand request, CancellationToken cancellationToken)
    {
        
    }
}
