using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Models.Results.User;

namespace VibraHeka.Application.Users.Queries.GetProfile;

public class GetUserProfileQueryHandler(ICurrentUserService currentUserService, IUserService userService) : IRequestHandler<GetUserProfileQuery, Result<UserDTO>>
{
    public Task<Result<UserDTO>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        return userService.GetUserByID(request.UserID, cancellationToken)
            .MapTry(user =>
            {
                UserDTO result = new UserDTO()
                {
                    Id = user.Id,
                    Bio = user.Bio,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    MiddleName = user.MiddleName,
                    LastName = user.LastName, 
                    TimezoneID = user.TimezoneID,
                    ProfilePictureUrl = user.ProfilePictureUrl,
                };

                if (currentUserService.UserId == user.Id)
                {
                    result.PhoneNumber = user.PhoneNumber;
                }

                return result;
            });
    }
}
