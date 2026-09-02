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
    
    protected const string NewPassword = "NewPassword123@";
    
    protected Task<HttpResponseMessage> InvokeAuthenticateEndpoint(AuthenticateUserRequest command) => Client.PostAsJsonAsync(LoginEndpoint, command);
    
    protected Task<HttpResponseMessage> InvokeChangePasswordEndpoint(ChangePasswordRequest command) => Client.PatchAsJsonAsync(ChangePasswordEndpoint, command);
    protected Task<HttpResponseMessage> InvokeConfirmResetPasswordEndpoint(ConfirmResetPasswordRequest command) => Client.PostAsJsonAsync(ConfirmResetPasswordEndpoint, command);
    
    protected Task<HttpResponseMessage> InvokeResetPasswordEndpoint(ResetPasswordRequest request) => Client.PostAsJsonAsync(ResetPasswordEndpoint, request);
    
    protected Task PerformChangePassword(ChangePasswordRequest command) => PerformCallAndExpectStatusCode(() => InvokeChangePasswordEndpoint(command), HttpStatusCode.NoContent);
    
    protected Task PerformResetPassword(ResetPasswordRequest request) => PerformCallAndExpectStatusCode(() => InvokeResetPasswordEndpoint(request), HttpStatusCode.NoContent);
    
    protected Task PerformConfirmResetPassword(ConfirmResetPasswordRequest command) => PerformCallAndExpectStatusCode(() => InvokeConfirmResetPasswordEndpoint(command), HttpStatusCode.NoContent);

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
