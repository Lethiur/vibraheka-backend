using CSharpFunctionalExtensions;

namespace VibraHeka.Application.Users.Commands.ResendConfirmationCode;

public record ResendConfirmationCodeCommand(string Email) : IRequest<Result<Unit>>;
