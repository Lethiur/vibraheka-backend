using CSharpFunctionalExtensions;

namespace VibraHeka.Application.Users.Commands.StartPasswordRecovery;

/// <summary>
/// Command used to start the password recovery flow for a user email.
/// </summary>
/// <param name="Email">User email that requests password recovery.</param>
public record StartPasswordRecoveryCommand(string Email) : IRequest<Result<Unit>>;
