using Riok.Mapperly.Abstractions;
using VibraHeka.Application.Users.Commands.AdminCreateTherapist;
using VibraHeka.Application.Users.Commands.UpdateUserProfile;
using VibraHeka.Domain.Entities;
using VibraHeka.Web.Users;

namespace VibraHeka.Web.Mappers;

[Mapper]
public partial class UserMapper
{
    [MapperIgnoreSource(nameof(UpdateProfileRequest.AdditionalProperties))]
    public partial UpdateUserProfileCommand ToUpdateProfileCommand(UpdateProfileRequest request);
    
    [MapValue(nameof(CreateTherapistCommand.Bio), "")]
    [MapValue(nameof(CreateTherapistCommand.ProfilePictureUrl), "")]
    [MapperIgnoreSource(nameof(UpdateProfileRequest.AdditionalProperties))]
    public partial CreateTherapistCommand ToCreateTherapistCommand(CreateTherapistRequest request);
    
    public partial UserDTO ToUserDto(UserEntity entity);
}
