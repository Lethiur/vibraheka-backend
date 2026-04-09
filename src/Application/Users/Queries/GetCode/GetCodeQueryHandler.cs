#if DEBUG
using CSharpFunctionalExtensions;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.User.Ports.output;

namespace VibraHeka.Application.Users.Queries.GetCode;

public class GetCodeQueryHandler(UserCodePort repo) : IRequestHandler<GetCodeQuery, Result<VerificationCodeEntity>>
{
    public Task<Result<VerificationCodeEntity>> Handle(GetCodeQuery request, CancellationToken cancellationToken)
    {
        return repo.GetCodeFor(request.UserName, cancellationToken);
    }
}
#endif
