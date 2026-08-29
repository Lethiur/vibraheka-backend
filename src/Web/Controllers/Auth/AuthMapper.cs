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
    [MapperIgnoreSource(nameof(RegisterUserRequest.AdditionalProperties))]
    public partial RegisterUserCommand ToCommand(RegisterUserRequest request);

    [MapperIgnoreSource(nameof(VerifyUserRequest.AdditionalProperties))]
    public partial VerifyUserCommand ToCommand(VerifyUserRequest request);
    
    [MapperIgnoreSource(nameof(AuthenticateUserRequest.AdditionalProperties))]
    public partial AuthenticateUserCommand ToCommand(AuthenticateUserRequest request);

    [MapperIgnoreSource(nameof(ResendConfirmationCodeRequest.AdditionalProperties))]
    public partial ResendConfirmationCodeCommand ToCommand(ResendConfirmationCodeRequest request);

    [MapperIgnoreSource(nameof(ResetPasswordRequest.AdditionalProperties))]
    public partial StartPasswordRecoveryCommand ToCommand(ResetPasswordRequest request);
    
    [MapperIgnoreSource(nameof(RefreshTokenRequest.AdditionalProperties))]
    public partial RefreshTokenCommand ToCommand(RefreshTokenRequest request);
    
    [MapperIgnoreSource(nameof(ConfirmResetPasswordRequest.AdditionalProperties))]
    public partial ConfirmPasswordRecoveryCommand ToCommand(ConfirmResetPasswordRequest request);
    
    [MapperIgnoreSource(nameof(ChangePasswordRequest.AdditionalProperties))]
    public partial ChangeAuthenticatedPasswordCommand ToCommand(ChangePasswordRequest request);
    
    [MapperIgnoreTarget(nameof(RegisterUserResponse.AdditionalProperties))]
    public partial RegisterUserResponse ToResponse(UserRegistrationResult result);

    [MapperIgnoreTarget(nameof(RegisterUserResponse.AdditionalProperties))]
    [MapperIgnoreSource(nameof(AuthenticationResult.UserID))]
    public partial AuthenticateUserResponse ToResponse(AuthenticationResult result);
    
    [MapEnum(EnumMappingStrategy.ByValue)]
    private partial UserRole ToResponse(Domain.Entities.UserRole role);
}
