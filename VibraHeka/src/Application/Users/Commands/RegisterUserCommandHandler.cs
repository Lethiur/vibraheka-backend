using VibraHeka.Application.Common.Interfaces;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Application.Users.Commands;

public class RegisterUserCommandHandler(ICognitoService cognito, IUserRepository users)
    : IRequestHandler<RegisterUserCommand, string>
{
    public async Task<string> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        if (await users.ExistsByEmailAsync(request.Email))
            throw new Exception("El usuario ya existe, vaquero.");

        var cognitoId = await cognito.RegisterUserAsync(request.Email, request.Password, request.FullName);

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = request.Email,
            FullName = request.FullName,
            CognitoId = cognitoId
        };

        await users.AddAsync(user);

        return user.Id;
    }
}
