using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Interfaces.User;

namespace VibraHeka.Application.Users.Commands.RefreshToken;

public class RefreshTokenCommandHandler(IUserService userService) : IRequestHandler<RefreshTokenCommand, Result<string>>
{
    
    public Task<Result<string>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        return userService.RefreshToken(request.RefreshToken, request.Email, cancellationToken);
    }
}
