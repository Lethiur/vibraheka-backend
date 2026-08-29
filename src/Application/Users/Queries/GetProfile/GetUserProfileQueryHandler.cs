using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Application.Users.Queries.GetProfile;

public class GetUserProfileQueryHandler(IUserService userService) : IRequestHandler<GetUserProfileQuery, Result<UserEntity>>
{
    public Task<Result<UserEntity>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        return userService.GetUserByID(request.UserID, cancellationToken);
    }
}
