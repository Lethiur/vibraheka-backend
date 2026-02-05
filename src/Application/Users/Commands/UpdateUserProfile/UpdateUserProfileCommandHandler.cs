using System.Diagnostics.CodeAnalysis;
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
        return Maybe.From(request.NewUserData)
            .Where(bool (userDTO) => userDTO.Id == request.NewUserData.Id)
            .ToResult(UserErrors.NotAuthorized)
            .MapTry(UserEntity (userDTO) => new UserEntity(userDTO.Id, userDTO.Email, userDTO.FirstName)
            {
                MiddleName = userDTO.MiddleName,
                LastName = userDTO.LastName,
                PhoneNumber = userDTO.PhoneNumber,
                Bio = userDTO.Bio
            })
            .BindTry(Task<Result<Unit>> (userDTO) => userService.UpdateUserAsync(userDTO, currentUserService.UserId!, cancellationToken));


    }
}
