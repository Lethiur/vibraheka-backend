using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Domain.Common.Interfaces.User;

public interface IUserCodeRepository
{
    Task<Result<Unit>> SaveCode(UserCodeEntity userCode, CancellationToken cancellationToken);
    
    Task<Result<UserCodeEntity>> GetCodeEntityFromEmail(string email, CancellationToken cancellationToken);
}
