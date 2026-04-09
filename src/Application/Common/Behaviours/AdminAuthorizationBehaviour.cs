using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.User.Ports.output;

namespace VibraHeka.Application.Common.Behaviours;

/// <summary>
/// A behavior pipeline implementation that enforces administrative authorization for
/// requests that implement the <see cref="IRequireAdmin"/> interface.
/// </summary>
/// <typeparam name="TRequest">The type of the request. Must implement the <see cref="IRequireAdmin"/> interface.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
/// <remarks>
/// This behavior ensures that the current user has administrative privileges before allowing
/// the request to proceed. If the user is not authorized, an <see cref="UnauthorizedException"/>
/// is thrown. Uses <see cref="ICurrentUserService"/> to retrieve the current user's identity
/// and <see cref="IPrivilegeService"/> to verify their role.
/// </remarks>
public sealed class AdminAuthorizationBehavior<TRequest, TResponse>(
    ICurrentUserService currentUser,
    UserPrivilegePort privilegeService)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequireAdmin
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        Result<bool> result = await Maybe
            .From(currentUser.UserId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .ToResult(UserErrors.InvalidUserID)
            .Bind(id => privilegeService.HasRoleAsync(id, UserRole.Admin, cancellationToken))
            .Ensure(hasRole => hasRole, UserErrors.NotAuthorized);

        if (result.IsFailure)
            throw new UnauthorizedException();

        return await next(cancellationToken);
    }
}
