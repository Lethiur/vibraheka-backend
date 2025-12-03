using CSharpFunctionalExtensions;

namespace VibraHeka.Application.Users.Commands;

/// <summary>
/// Represents a command used to register a new user.
/// </summary>
/// <remarks>
/// The command encapsulates the user's email, password, and full name.
/// It is designed to be handled by an appropriate command handler,
/// ensuring implementation details are managed separately.
/// </remarks>
public record RegisterUserCommand(string Email, string Password, string FullName) : IRequest<Result<string>>;
