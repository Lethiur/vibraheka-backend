using CSharpFunctionalExtensions;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Application.Users.Queries.GetCode;

public class GetCodeQuery(string userName) : IRequest<Result<VerificationCodeEntity>>
{
    public string UserName { get; set; } = userName;
}
