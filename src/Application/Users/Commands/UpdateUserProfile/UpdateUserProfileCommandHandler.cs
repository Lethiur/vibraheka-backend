using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;


namespace VibraHeka.Application.Users.Commands.UpdateUserProfile;

public class UpdateUserCommandHandler(ICurrentUserService currentUserService, IUserService userService) : IRequestHandler<UpdateUserProfileCommand, Result<Unit>>
{
    public Task<Result<Unit>> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        return Maybe.From(request)
            .ToResult(UserErrors.NotAuthorized)
            .MapTry(command => new UserEntity(currentUserService.UserId!, command.Email, command.FirstName)
            {
                MiddleName = command.MiddleName,
                LastName = command.LastName,
                PhoneNumber = command.PhoneNumber,
                Bio = command.Bio,
                TimezoneID = command.TimezoneID,
            })
            .BindTry(Task<Result<Unit>> (userDto) => userService.UpdateUserAsync(userDto, currentUserService.UserId!, cancellationToken));


    }
}
