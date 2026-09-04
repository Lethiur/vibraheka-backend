using System.Net;
using System.Net.Http.Json;
using VibraHeka.Web.AcceptanceTests.Generic;
using VibraHeka.Web.Authentication;

namespace VibraHeka.Web.AcceptanceTests.Auth;

public class GenericAuthAcceptanceTest : GenericAcceptanceTest<VibraHekaProgram>
{
    protected const string RefreshTokenEndpoint = "/api/v1/auth/refresh-token";
    protected const string ChangePasswordEndpoint = "/api/v1/auth";
    protected const string ResendVerificationCodeEndpoint = "/api/v1/auth/resend-confirmation-code";
    protected const string ResetPasswordEndpoint = "/api/v1/auth/reset-password";
    protected const string ConfirmResetPasswordEndpoint = "/api/v1/auth/reset-password/confirm";
    protected const string VerifyRegistrationEndpoint = "/api/v1/auth/verify";
    
    protected const string NewPassword = "NewPassword123@";
    
    protected Task<HttpResponseMessage> InvokeRegistrationEndpoint(RegisterUserRequest command) => Client.PutAsJsonAsync(RegisterEndpoint, command);
    
    protected Task<HttpResponseMessage> InvokeAuthenticateEndpoint(AuthenticateUserRequest command) => Client.PostAsJsonAsync(LoginEndpoint, command);
    
    protected Task<HttpResponseMessage> InvokeChangePasswordEndpoint(ChangePasswordRequest command) => Client.PatchAsJsonAsync(ChangePasswordEndpoint, command);
    
    protected Task<HttpResponseMessage> InvokeConfirmResetPasswordEndpoint(ConfirmResetPasswordRequest command) => Client.PostAsJsonAsync(ConfirmResetPasswordEndpoint, command);
    
    protected Task<HttpResponseMessage> InvokeResetPasswordEndpoint(ResetPasswordRequest request) => Client.PostAsJsonAsync(ResetPasswordEndpoint, request);
    
    protected Task<HttpResponseMessage> InvokeRefreshTokenEndpoint(RefreshTokenRequest command) => Client.PostAsJsonAsync(RefreshTokenEndpoint, command);
    
    protected Task<HttpResponseMessage> InvokeResendConfirmationCodeEndpoint(ResendConfirmationCodeRequest request) => Client.PostAsJsonAsync(ResendVerificationCodeEndpoint, request);
    
    protected Task<HttpResponseMessage> InvokeVerifyRegistrationEndpoint(VerifyUserRequest request) => Client.PatchAsJsonAsync(VerifyRegistrationEndpoint, request);
    
    protected Task PerformRegistration(RegisterUserRequest command) => PerformCallAndExpectStatusCode(() => InvokeRegistrationEndpoint(command), HttpStatusCode.OK);
    
    protected Task PerformChangePassword(ChangePasswordRequest command) => PerformCallAndExpectStatusCode(() => InvokeChangePasswordEndpoint(command), HttpStatusCode.NoContent);
    
    protected Task PerformResetPassword(ResetPasswordRequest request) => PerformCallAndExpectStatusCode(() => InvokeResetPasswordEndpoint(request), HttpStatusCode.NoContent);
    
    protected Task PerformConfirmResetPassword(ConfirmResetPasswordRequest command) => PerformCallAndExpectStatusCode(() => InvokeConfirmResetPasswordEndpoint(command), HttpStatusCode.NoContent);
    
    protected Task PerformVerifyRegistration(VerifyUserRequest request) => PerformCallAndExpectStatusCode(() => InvokeVerifyRegistrationEndpoint(request), HttpStatusCode.NoContent);
    
    protected Task PerformResendConfirmationCode(ResendConfirmationCodeRequest request) => PerformCallAndExpectStatusCode(() => InvokeResendConfirmationCodeEndpoint(request), HttpStatusCode.NoContent);

    protected AuthenticateUserRequest BuildWithRegularPassword(ref string email)
    {
        return new AuthenticateUserRequest() {Email = email, Password = ThePassword};
    }
    
    protected AuthenticateUserRequest BuildAuthenticateUserRequest(ref string email, string password)
    {
        return new AuthenticateUserRequest() {Email = email, Password = password};
    }

    protected ChangePasswordRequest BuildChangePasswordRequestCorrectly()
    {
        ChangePasswordRequest request = new() { CurrentPassword = ThePassword, NewPassword = NewPassword, NewPasswordConfirmation = NewPassword };
        return request;
    }
}
