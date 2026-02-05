using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Models.Results;

namespace VibraHeka.Application.Users.Commands.RegisterUser;

/// <summary>
/// Handles the execution of the RegisterUserCommand, which is responsible for registering
/// a new user by interacting with Cognito authentication services and persisting the user in the system.
/// </summary>
/// <remarks>
/// The handler performs the following operations:
/// 1. Calls the Cognito service to register the user credentials and retrieve a Cognito ID.
/// 2. Binds the result to create a new user entity with the retrieved Cognito ID.
/// 3. Persists the user to the user repository.
/// 4. Returns a result indicating the success of the registration, including whether confirmation is required.
/// </remarks>
/// <param name="user">Abstraction for interacting with the AWS Cognito service.</param>
/// <param name="users">Repository interface for user persistence operations such as adding a user.</param>
public class RegisterUserCommandHandler(IUserService user, IUserRepository users)
    : IRequestHandler<RegisterUserCommand, Result<UserRegistrationResult>>
{
    /// <summary>
    /// Handles the user registration process including communication with the Cognito service
    /// and storage of user information in the repository.
    /// </summary>
    /// <param name="request">
    /// An instance of <see cref="RegisterUserCommand"/> containing the user's email, password, and full name.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to propagate notification that the operation should be canceled.
    /// </param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing a <see cref="UserRegistrationResult"/> with user registration details
    /// or an error if the registration process fails.
    /// </returns>
    public async Task<Result<UserRegistrationResult>> Handle(RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        Result<string> cognitoId = await user.RegisterUserAsync(request.Email, request.Password, request.FullName);
        
        return await cognitoId.Bind(async realCognitoId =>
        {
            UserEntity newUserEntity = new()
            {
                Id =realCognitoId,
                Email = request.Email,
                FirstName = request.FullName,
                CognitoId = realCognitoId,
                Created = DateTime.UtcNow,
                LastModified = DateTime.UtcNow,
            };

            Result<string> addAsync = await users.AddAsync(newUserEntity);

            return addAsync.Match(userId => Result.Success(new UserRegistrationResult(userId, true)), Result.Failure<UserRegistrationResult>);
        });
    }
}
