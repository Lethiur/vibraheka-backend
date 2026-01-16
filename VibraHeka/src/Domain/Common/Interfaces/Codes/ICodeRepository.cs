using CSharpFunctionalExtensions;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Domain.Common.Interfaces.Codes;

public interface ICodeRepository
{
   Task<Result<VerificationCodeEntity>> GetCodeFor(string email);
}
