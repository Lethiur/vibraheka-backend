using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Interfaces;
using VibraHeka.Domain.Entities;
namespace VibraHeka.Application.Users.Commands;

public class RegisterUserCommandHandler(ICognitoService cognito, IUserRepository users)
    : IRequestHandler<RegisterUserCommand, Result<string>>
{
    public async Task<Result<string>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        Result<string> cognitoId = await cognito.RegisterUserAsync(request.Email, request.Password, request.FullName);
        
        
        return await cognitoId.Bind(async realCognitoId =>
        {
            User user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Email = request.Email,
                FullName = request.FullName,
                CognitoId = realCognitoId
            };

            return await users.AddAsync(user);
        });
    }
}
