using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Models.Results;

namespace VibraHeka.Application.Users.Commands.AuthenticateUsers;

/// <summary>
/// Represents a command to authenticate a user, requiring the user's email and password.
/// Processes authentication and returns a result containing authentication details or an error.
/// </summary>
public record AuthenticateUserCommand(string Email, string Password) : IRequest<Result<AuthenticationResult>>;
