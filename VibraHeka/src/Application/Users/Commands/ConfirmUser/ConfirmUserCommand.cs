using CSharpFunctionalExtensions;

namespace Microsoft.Extensions.DependencyInjection.Users.Commands.ConfirmUser;

/// <summary>
/// Represents a command to confirm the identity of a user.
/// </summary>
public record ConfirmUserCommand(string Email, string Code) : IRequest<Result<Unit>>;
