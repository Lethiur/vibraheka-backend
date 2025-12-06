using CSharpFunctionalExtensions;

namespace VibraHeka.Application.Users.Commands.VerificationCode;

public record VerifyUserCommand(string Email, string Code) : IRequest<Result<Unit>>;
