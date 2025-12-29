using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Interfaces;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Application.Users.Queries.GetCode;

public class GetCodeQueryHandler(ICodeRepository repo) : IRequestHandler<GetCodeQuery, Result<VerificationCodeEntity>>
{
    public Task<Result<VerificationCodeEntity>> Handle(GetCodeQuery request, CancellationToken cancellationToken)
    {
        return repo.GetCodeFor(request.UserName);
    }
}
