using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Interfaces;

namespace Microsoft.Extensions.DependencyInjection.Users.Commands.ConfirmUser;

/// <summary>
/// 
/// </summary>
/// <param name="cognitoService"></param>
public class ConfirmUserCommandHandler(ICognitoService cognitoService) : IRequestHandler<ConfirmUserCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(ConfirmUserCommand request, CancellationToken cancellationToken)
    {
        return await cognitoService.ConfirmUserAsync(request.Email, request.Code).WaitAsync(cancellationToken);
    }
}
