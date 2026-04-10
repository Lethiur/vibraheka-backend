using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.User.Ports.Output;
using VibraHeka.Domain.User.Services;


namespace VibraHeka.Application.Users.Commands.UpdateUserProfile;

public class UpdateUserCommandHandler(ICurrentUserService currentUserService, UserProfilePort userService) : IRequestHandler<UpdateUserProfileCommand, Result<Unit>>
{
    public Task<Result<Unit>> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        return Maybe.From(request.NewUserData)
            .Where(userDTO => userDTO.Id == currentUserService.UserId)
            .ToResult(UserErrors.NotAuthorized)
            .MapTry(UserProfileEntity (userDTO) => new UserProfileEntity(userDTO.Id, userDTO.Email, userDTO.FirstName)
            {
                MiddleName = userDTO.MiddleName,
                LastName = userDTO.LastName,
                PhoneNumber = userDTO.PhoneNumber,
                Bio = userDTO.Bio
            })
            .BindTry(Task<Result<Unit>> (userDTO) => userService.UpdateUserProfile(userDTO, currentUserService.UserId!, cancellationToken));


    }
}
