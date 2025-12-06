using CSharpFunctionalExtensions;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Application.Common.Interfaces;

public interface ICodeRepository
{
   Task<Result<VerificationCodeEntity>> GetCodeFor(string email);
}
