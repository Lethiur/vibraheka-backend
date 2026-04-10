using CSharpFunctionalExtensions;
using VibraHeka.Domain.User.Ports.Output;

namespace VibraHeka.Application.Users.Commands.RefreshToken;

public class RefreshTokenCommandHandler(UserPort userService) : IRequestHandler<RefreshTokenCommand, Result<string>>
{
    
    public Task<Result<string>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        return userService.RefreshToken(request.RefreshToken, request.Email, cancellationToken);
    }
}
