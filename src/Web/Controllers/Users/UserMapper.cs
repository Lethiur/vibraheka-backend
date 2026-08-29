using Riok.Mapperly.Abstractions;
using VibraHeka.Application.Users.Commands.AdminCreateTherapist;
using VibraHeka.Application.Users.Commands.UpdateUserProfile;
using VibraHeka.Domain.Entities;
using VibraHeka.Web.Users;
using UserRole = VibraHeka.Web.Users.UserRole;


namespace VibraHeka.Web.Controllers.Users;

[Mapper]
public partial class UserMapper
{
    [MapperIgnoreSource(nameof(UpdateProfileRequest.AdditionalProperties))]
    public partial UpdateUserProfileCommand ToUpdateProfileCommand(UpdateProfileRequest request);
    
    [MapValue(nameof(CreateTherapistCommand.Bio), "")]
    [MapValue(nameof(CreateTherapistCommand.ProfilePictureUrl), "")]
    [MapperIgnoreSource(nameof(CreateTherapistRequest.AdditionalProperties))]
    public partial CreateTherapistCommand ToCreateTherapistCommand(CreateTherapistRequest request);
    
    [MapEnum(EnumMappingStrategy.ByValue)]
    private partial UserRole ToResponse(Domain.Entities.UserRole role);
    
    
    [MapperIgnoreSource(nameof(UserEntity.CustomerID))]
    [MapperIgnoreSource(nameof(UserEntity.ProfilePictureUrl))]
    [MapperIgnoreSource(nameof(UserEntity.Created))]
    [MapperIgnoreSource(nameof(UserEntity.CreatedBy))]
    [MapperIgnoreSource(nameof(UserEntity.LastModified))]
    [MapperIgnoreSource(nameof(UserEntity.LastModifiedBy))]
    [MapperIgnoreTarget(nameof(UserDTO.AdditionalProperties))]
    public partial UserDTO ToUserDto(UserEntity entity);
    
    
}
