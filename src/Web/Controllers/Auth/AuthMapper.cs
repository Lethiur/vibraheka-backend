using Riok.Mapperly.Abstractions;
using VibraHeka.Application.Users.Commands.AuthenticateUsers;
using VibraHeka.Application.Users.Commands.ChangeAuthenticatedPassword;
using VibraHeka.Application.Users.Commands.ConfirmPasswordRecovery;
using VibraHeka.Application.Users.Commands.RefreshToken;
using VibraHeka.Application.Users.Commands.RegisterUser;
using VibraHeka.Application.Users.Commands.ResendConfirmationCode;
using VibraHeka.Application.Users.Commands.StartPasswordRecovery;
using VibraHeka.Application.Users.Commands.VerificationCode;
using VibraHeka.Domain.Models.Results;
using VibraHeka.Web.Authentication;

namespace VibraHeka.Web.Controllers.Auth;

[Mapper]
public partial class AuthMapper
{
    public partial RegisterUserCommand ToCommand(RegisterUserRequest request);
    
    public partial VerifyUserCommand ToCommand(VerifyUserRequest request);
    
    public partial AuthenticateUserCommand ToCommand(AuthenticateUserRequest request);
    
    public partial ResendConfirmationCodeCommand ToCommand(ResendConfirmationCodeRequest request);
    
    public partial StartPasswordRecoveryCommand ToCommand(ResetPasswordRequest request);
    
    public partial RefreshTokenCommand ToCommand(RefreshTokenRequest request);
    
    public partial ConfirmPasswordRecoveryCommand ToCommand(ConfirmResetPasswordRequest request);
    
    public partial ChangeAuthenticatedPasswordCommand ToCommand(ChangePasswordRequest request);
    
    public partial RegisterUserResponse ToResponse(UserRegistrationResult result);
    
    [MapperIgnoreSource(nameof(AuthenticationResult.UserID))]
    public partial AuthenticateUserResponse ToResponse(AuthenticationResult result);
    
    [MapEnum(EnumMappingStrategy.ByValue)]
    private partial UserRole ToResponse(Domain.Entities.UserRole role);
}
