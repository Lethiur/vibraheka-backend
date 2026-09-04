using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Core.Internal.Entities;
using Bogus;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using VibraHeka.Application.Users.Commands.AuthenticateUsers;
using VibraHeka.Application.Users.Commands.VerificationCode;
using VibraHeka.Application.Users.Queries.GetCode;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Web.AcceptanceTests.Utils;
using VibraHeka.Web.Authentication;
using UserRole = VibraHeka.Domain.Entities.UserRole;

namespace VibraHeka.Web.AcceptanceTests.Generic;

public class GenericAcceptanceTest<TAppClass> where TAppClass : class
{
    private const string GetVerificationCodeEndpoint = "/api/v1/codes/verification-code";
    protected const string LoginEndpoint = "/api/v1/auth/authenticate";
    protected const string VerifyEndpoint = "/api/v1/auth/verify";
    protected const string RegisterEndpoint = "/api/v1/auth/register";
   

    protected const string ThePassword = "Password123@";
    
    protected HttpClient Client = null!;
    protected Faker TheFaker;
    private readonly WebApplicationFactory<TAppClass> Factory;

    public GenericAcceptanceTest()
    {
        TheFaker = new Faker();
        Factory = new WebApplicationFactory<TAppClass>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Test");
            });
    }

    [SetUp]
    public void Setup()
    {
        AWSXRayRecorder.Instance.TraceContext.SetEntity(new Segment("VH-ACCEPTANCE-TEST"));
        Client = Factory.CreateClient();
    }

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        Client = Factory.CreateClient();
    }

    [TearDown]
    public void Teardown()
    {
        Client.Dispose();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        Client.Dispose();
        Factory.Dispose();
    }

    /// <summary>
    /// Waits for a verification code to be generated for a specified item within a given timeout period.
    /// </summary>
    /// <param name="itemId">The identifier of the item for which the verification code is being requested.</param>
    /// <param name="timeout">The maximum duration to wait for the verification code to become available.</param>
    /// <returns>An instance of <c>VerificationCodeEntity</c> that contains the generated verification code.</returns>
    /// <exception cref="TimeoutException">Thrown when the verification code is not available within the specified timeout period.</exception>
    protected async Task<VerificationCodeEntity> WaitForVerificationCode(string itemId, TimeSpan timeout)
    {
        DateTime startTime = DateTime.UtcNow;
        while (DateTime.UtcNow - startTime < timeout)
        {
            try
            {
                HttpResponseMessage response =
                    await Client.PostAsJsonAsync(GetVerificationCodeEndpoint, new GetCodeQuery(itemId));

                VerificationCodeEntity asResponseEntityAndContentAs =
                    await response.ParseContentAsync<VerificationCodeEntity>();

                if (asResponseEntityAndContentAs.Code == string.Empty)
                {
                    continue;
                }

                return asResponseEntityAndContentAs;
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                Console.WriteLine($"Exception while waiting for verification code: {ex.Message}");
            }

            await Task.Delay(500); // Wait before retrying
        }

        throw new TimeoutException("DynamoDB record was not available within the expected time.");
    }


    /// <summary>
    /// Registers a new user by submitting their username, email, and password to the registration endpoint.
    /// </summary>
    /// <param name="email">The email address of the user to be registered.</param>
    /// <param name="password">The password for the user account being registered.</param>
    /// <returns>The unique identifier of the newly registered user.</returns>
    protected async Task<string> RegisterUser(string email, string password = ThePassword)
    {
        HttpResponseMessage postAsJsonAsync = await Client.PutAsJsonAsync(RegisterEndpoint,
            new RegisterUserRequest
            {
                Email = email,
                Password = password,
                FirstName = "Test",
                MiddleName = "TEST",
                LastName = "Test",
                TimezoneID = "Europe/Madrid"
            });

        RegisterUserResponse asResponseEntityAndContentAs =
            await postAsJsonAsync.ParseContentAsync<RegisterUserResponse>();

        return asResponseEntityAndContentAs.UserId;
    }

    /// <summary>
    /// Registers a new user, confirms their registration using a verification code, and persists the user in the system.
    /// </summary>
    /// <param name="email">The email address of the user to be registered and confirmed.</param>
    /// <param name="password">The password for the user account being registered and confirmed.</param>
    /// <returns>The unique identifier of the newly registered and confirmed user.</returns>
    protected async Task<string> RegisterAndConfirmUser(string email, string password = ThePassword)
    {
        string userID = await RegisterUser(email, password);
        VerificationCodeEntity codeResult = await WaitForVerificationCode(email, TimeSpan.FromSeconds(10));
        VerifyUserCommand verificationCommand = new(codeResult.Code);
        HttpResponseMessage patchAsJsonAsync =
            await Client.PatchAsJsonAsync(VerifyEndpoint, verificationCommand);
        patchAsJsonAsync.EnsureSuccessStatusCode();
        return userID;
    }

    /// <summary>
    /// Registers a new user as an administrator, waits for email verification, and confirms the user's admin status.
    /// </summary>
    /// <param name="username">The username of the administrator to be registered.</param>
    /// <param name="email">The email address of the administrator to be registered.</param>
    /// <param name="password">The password for the administrator's account.</param>
    /// <returns>The unique identifier of the newly registered administrator.</returns>
    /// <exception cref="HttpRequestException">Thrown when there is an issue with the HTTP request during user registration or promotion.</exception>
    /// <exception cref="TimeoutException">Thrown when the verification code is not retrieved within the specified timeout period.</exception>
    protected async Task RegisterAndConfirmAdmin(string username, string email, string password)
    {
        string userID = await RegisterAndConfirmUser(email, password);
        await PromoteToAdmin(username, email, userID);
    }

    /// <summary>
    /// Registers a new admin user, confirms their account, and logs them in.
    /// </summary>
    /// <param name="username">The username for the new admin user.</param>
    /// <param name="email">The email address for the new admin user.</param>
    /// <param name="password">The password for the new admin user.</param>
    /// <returns>An instance of <c>AuthenticateUserResponse</c> containing the authentication tokens and user role upon successful login.</returns>
    protected async Task RegisterAndConfirmAndLoginAdmin(string username = "", string email = "",
        string password = ThePassword)
    {
        await RegisterAndConfirmAdmin(username, email, password);
        await AuthenticateUser(email, password);
    }

    /// <summary>
    /// Registers a user with the specified credentials, confirms their account through verification,
    /// and logs them in to retrieve authentication tokens.
    /// </summary>
    /// <param name="email">The email address of the user, which will also receive the verification code.</param>
    /// <param name="password">The password for the user's account.</param>
    /// <returns>An instance of <c>AuthenticateUserResponse</c> containing the user's authentication information, including tokens and roles.</returns>
    /// <exception cref="HttpRequestException">Thrown when the confirmation or authentication process encounters an HTTP error.</exception>
    protected async Task<AuthenticateUserResponse> RegisterConfirmAndLogin(string email,
        string password = ThePassword)
    {
        await RegisterUser(email, password);
        VerificationCodeEntity codeResult = await WaitForVerificationCode(email, TimeSpan.FromSeconds(10));
        VerifyUserCommand verificationCommand = new(codeResult.Code);
        HttpResponseMessage patchAsJsonAsync =
            await Client.PatchAsJsonAsync(VerifyEndpoint, verificationCommand);
        patchAsJsonAsync.EnsureSuccessStatusCode();
        return await AuthenticateUser(email, password);
    }

    /// <summary>
    /// Authenticates a user by validating their credentials and retrieving authentication details upon success.
    /// </summary>
    /// <param name="email">The email address of the user attempting to authenticate.</param>
    /// <param name="password">The password associated with the user's account.</param>
    /// <returns>An instance of <c>AuthenticationResult</c> containing the user ID, access token, and refresh token.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the authentication result is null, indicating a failure to authenticate the user.</exception>
    protected async Task<AuthenticateUserResponse> AuthenticateUser(string email, string password)
    {
        AuthenticateUserCommand command = new(email, password);
        HttpResponseMessage response = await Client.PostAsJsonAsync(LoginEndpoint, command);
        AuthenticateUserResponse token = await response.ParseContentAsync<AuthenticateUserResponse>();
        Assert.That(token, Is.Not.Null);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        return token ?? throw new InvalidOperationException("Authentication result was null.");
    }

    /// <summary>
    /// Creates a new admin user and persists it in the repository.
    /// </summary>
    /// <param name="username">The full name of the admin user to be created.</param>
    /// <param name="email">The email address of the admin user.</param>
    /// <param name="ID">The ID of the user to promote to admin</param>
    /// <returns>The unique identifier of the newly created admin user.</returns>
    private async Task PromoteToAdmin(string username, string email, string ID)
    {
        using IServiceScope scope = Factory.Services.CreateScope();
        IUserRepository repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        string userId = Guid.NewGuid().ToString();
        UserEntity adminUserEntity = new()
        {
            Id = ID,
            Email = email,
            FirstName = username,
            Role = UserRole.Admin,
            Created = DateTime.UtcNow,
            CreatedBy = userId,
            LastModified = DateTime.UtcNow,
            LastModifiedBy = userId
        };

        await repository.AddAsync(adminUserEntity);
    }

    /// <summary>
    /// Retrieves a user entity by their unique identifier from the user repository.
    /// </summary>
    /// <param name="userID">The unique identifier of the user to be retrieved.</param>
    /// <returns>A <see cref="UserEntity"/> object representing the user associated with the specified identifier, or null if the user is not found.</returns>
    protected async Task<UserEntity> CheckForUser(string userID)
    {
        using IServiceScope scope = Factory.Services.CreateScope();
        IUserRepository repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        Result<UserEntity> user = await repository.GetByIdAsync(userID, CancellationToken.None);
        return user.GetValueOrDefault();
    }

    /// <summary>
    /// Retrieves an instance of the specified type from the underlying service factory.
    /// </summary>
    /// <typeparam name="T">The type of the object to retrieve from the factory. Must not be null.</typeparam>
    /// <returns>An instance of the requested type <c>T</c> resolved from the service provider.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the requested type <c>T</c> is not registered in the service provider.</exception>
    protected T GetObjectFromFactory<T>() where T : notnull
    {
        IServiceScope scope = Factory.Services.CreateScope();
        T obj = scope.ServiceProvider.GetRequiredService<T>();

        return obj;
    }

    /// <summary>
    /// Creates an encrypted verification token using the same algorithm as <c>PasswordResetTokenService</c>.
    /// Intended for use in acceptance tests that need to call the verify-account endpoint.
    /// </summary>
    /// <param name="email">Email to embed in the token.</param>
    /// <param name="cognitoCode">Cognito verification code to embed.</param>
    /// <param name="expiresAt">Optional expiry; defaults to 30 minutes from now.</param>
    /// <returns>Encrypted token string in the <c>v1.&lt;base64url&gt;</c> format.</returns>
    protected string CreateEncryptedToken(string email, string cognitoCode, DateTimeOffset? expiresAt = null)
    {
        AWSConfig config = GetObjectFromFactory<AWSConfig>();
        byte[] key = SHA256.HashData(Encoding.UTF8.GetBytes(config.PasswordResetTokenSecret.Trim()));

        byte[] plainText = JsonSerializer.SerializeToUtf8Bytes(new
        {
            Email = email,
            CognitoCode = cognitoCode,
            TokenId = Guid.NewGuid().ToString(),
            ExpiresAtUnix = (expiresAt ?? DateTimeOffset.UtcNow.AddMinutes(30)).ToUnixTimeSeconds()
        });

        byte[] nonce = new byte[12];
        RandomNumberGenerator.Fill(nonce);
        byte[] cipherText = new byte[plainText.Length];
        byte[] tag = new byte[16];

        using AesGcm aes = new(key, 16);
        aes.Encrypt(nonce, plainText, cipherText, tag);

        byte[] combined = [.. nonce, .. tag, .. cipherText];
        string base64Url = Convert.ToBase64String(combined)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

        return $"v1.{base64Url}";
    }

    /// <summary>
    /// Removes the "Authorization" header from the default request headers of the HTTP client, effectively clearing any existing authentication context for subsequent requests.
    /// </summary>
    protected void RemoveAuthHeader()
    {
        Client.DefaultRequestHeaders.Remove("Authorization");
    }

    /// <summary>
    /// Authenticates a new user by generating a unique email, registering the user, confirming their email, and logging them in.
    /// </summary>
    /// <returns>An instance of <c>AuthenticateUserResponse</c> containing the authentication details of the newly created user.</returns>
    /// <exception cref="HttpRequestException">Thrown if there is an issue during the registration, confirmation, or login process.</exception>
    protected async Task<AuthenticateUserResponse> AuthenticateAsNewUser()
    {
        string email = TheFaker.Internet.Email();
        return await RegisterConfirmAndLogin(email, ThePassword);
    }

    /// <summary>
    /// Authenticates a newly registered administrator by registering, confirming,
    /// and logging in the administrator with a randomly generated email and predefined password.
    /// </summary>
    /// <returns>An instance of <c>AuthenticateUserResponse</c> containing the access token, refresh token, and user role of the authenticated administrator.</returns>
    protected async Task AuthenticateAsNewAdmin()
    {
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmAdmin(TheFaker.Person.FullName, email, ThePassword);
        await AuthenticateUser(email, ThePassword);
    }
    
    /// <summary>
    /// Executes an HTTP action and verifies that it results in a bad request with the specified error code.
    /// </summary>
    /// <param name="action">The asynchronous HTTP action to be performed.</param>
    /// <param name="expectedErrorCode">The error code expected in the response.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="AssertionException">Thrown if the response does not contain the expected error code or if the status code is not BadRequest.</exception>
    protected static async Task PerformCallAndExpectError(Func<Task<HttpResponseMessage>> action,
        string expectedErrorCode)
    {
        HttpResponseMessage response = await PerformCallAndExpectStatusCode(action, HttpStatusCode.BadRequest);
        await response.AssertBadRequestWithError(expectedErrorCode);
    }

    /// <summary>
    /// Retrieves the user's unique identifier (user ID) from the authentication token in the request header.
    /// </summary>
    /// <returns>A <c>Guid</c> representing the user ID extracted from the token.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no authentication header is found in the request.</exception>
    protected Guid GetuserID()
    {
        AuthenticationHeaderValue? authenticationHeaderValue = Client.DefaultRequestHeaders.Authorization;
        if (authenticationHeaderValue == null)
        {
            throw new InvalidOperationException("No authentication header found.");
        }
        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(authenticationHeaderValue.Parameter);
        return Guid.Parse(jwtSecurityToken.Subject);
    }

    /// <summary>
    /// Executes a specified HTTP call and retrieves the response content after validating the expected status code.
    /// </summary>
    /// <typeparam name="T">The type to which the response content will be deserialized.</typeparam>
    /// <param name="action">A function representing the HTTP call to be performed.</param>
    /// <returns>The deserialized content of type <c>T</c> extracted from the HTTP response.</returns>
    /// <exception cref="AssertionException">Thrown when the status code of the response does not match the expected status code.</exception>
    /// <exception cref="HttpRequestException">Thrown when an error occurs while making the HTTP call.</exception>
    protected static async Task<T> PerformCallAndRetrieveContent<T>(Func<Task<HttpResponseMessage>> action)
    {
        HttpResponseMessage response = await PerformCallAndExpectStatusCode(action, HttpStatusCode.OK);
        T content = await response.ParseContentAsync<T>();
        return content;
    }
    

    /// <summary>
    /// Executes an HTTP request and verifies that the response status code matches the expected status code.
    /// </summary>
    /// <param name="action">A function that performs the HTTP request and returns an <c>HttpResponseMessage</c> asynchronously.</param>
    /// <param name="expectedStatusCode">The expected HTTP status code to be validated against the actual response.</param>
    /// <returns>A <c>Task</c> that represents the asynchronous operation.</returns>
    /// <exception cref="AssertionException">Thrown if the actual response status code does not match the expected status code.</exception>
    protected static async Task<HttpResponseMessage> PerformCallAndExpectStatusCode(Func<Task<HttpResponseMessage>> action,
        HttpStatusCode expectedStatusCode)
    {
        HttpResponseMessage response = await action();
        Assert.That(response.StatusCode, Is.EqualTo(expectedStatusCode));
        return response;
    }
}
