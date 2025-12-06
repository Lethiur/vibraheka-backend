using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Interfaces;
using VibraHeka.Application.Common.Models.Results;
using VibraHeka.Domain.Entities;
namespace VibraHeka.Application.Users.Commands;

public class RegisterUserCommandHandler(ICognitoService cognito, IUserRepository users)
    : IRequestHandler<RegisterUserCommand, Result<UserRegistrationResult>>
{
    public async Task<Result<UserRegistrationResult>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
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

            Result<string> addAsync = await users.AddAsync(user);

            return addAsync.Match(userId => Result.Success(new UserRegistrationResult(userId, true)), Result.Failure<UserRegistrationResult>);
        });
    }
}
