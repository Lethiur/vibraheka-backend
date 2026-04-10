using CSharpFunctionalExtensions;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Domain.User.Ports.Out;

public interface UserCodePort
{
    Task<Result<VerificationCodeEntity>> GetCodeFor(string email, CancellationToken cancellationToken);
}
