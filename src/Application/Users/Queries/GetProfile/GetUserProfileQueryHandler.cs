using CSharpFunctionalExtensions;
using VibraHeka.Domain.Models.Results.User;
using VibraHeka.Domain.User.Ports.Output;
using VibraHeka.Domain.User.Services;

namespace VibraHeka.Application.Users.Queries.GetProfile;

public class GetUserProfileQueryHandler(ICurrentUserService currentUserService, UserProfilePort userService) : IRequestHandler<GetUserProfileQuery, Result<UserDTO>>
{
    public Task<Result<UserDTO>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        return userService.GetProfileByUserId(request.UserID, cancellationToken)
            .MapTry(user =>
            {
                UserDTO result = new()
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
