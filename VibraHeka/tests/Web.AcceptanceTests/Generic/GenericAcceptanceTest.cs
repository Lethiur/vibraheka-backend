using System.Net.Http.Json;
using Bogus;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using VibraHeka.Application.Users.Commands.AuthenticateUsers;
using VibraHeka.Application.Users.Commands.RegisterUser;
using VibraHeka.Application.Users.Commands.VerificationCode;
using VibraHeka.Application.Users.Queries.GetCode;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Models.Results;

namespace VibraHeka.Web.AcceptanceTests.Generic;

public class GenericAcceptanceTest<TAppClass> where TAppClass : class
{
    protected HttpClient Client = null!;
    protected Faker TheFaker;
    protected string ThePassword = "Password123@";
    private readonly WebApplicationFactory<TAppClass> Factory;

    public GenericAcceptanceTest()
    {
        TheFaker = new Faker();
        Factory = new WebApplicationFactory<TAppClass>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    // Borra config anterior
                    configBuilder.Sources.Clear();

                    configBuilder.AddJsonFile("appsettings.Development.json", optional: false)
                        .AddEnvironmentVariables();
                });
            });
    }

    [SetUp]
    public void Setup()
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
            HttpResponseMessage response =
                await Client.PostAsJsonAsync($"api/v1/auth/verification-code", new GetCodeQuery(itemId));
            ResponseEntity asResponseEntityAndContentAs =
                await response.GetAsResponseEntityAndContentAs<VerificationCodeEntity>();
            VerificationCodeEntity? registerUserResponse =
                asResponseEntityAndContentAs.GetContentAs<VerificationCodeEntity>();
            if (registerUserResponse != null) return registerUserResponse;

            await Task.Delay(500); // Wait before retrying
        }

        throw new TimeoutException("DynamoDB record was not available within the expected time.");
    }


    /// <summary>
    /// Registers a new user by submitting their username, email, and password to the registration endpoint.
    /// </summary>
    /// <param name="username">The username of the user to be registered.</param>
    /// <param name="email">The email address of the user to be registered.</param>
    /// <param name="password">The password for the user account being registered.</param>
    /// <returns>The unique identifier of the newly registered user.</returns>
    protected async Task<string> RegisterUser(string username, string email, string password)
    {
        HttpResponseMessage postAsJsonAsync = await Client.PostAsJsonAsync("api/v1/auth/register",
            new RegisterUserCommand(
                email, password, username)
        );
        ResponseEntity asResponseEntityAndContentAs =
            await postAsJsonAsync.GetAsResponseEntityAndContentAs<UserRegistrationResult>();
        UserRegistrationResult? registerUserResponse =
            asResponseEntityAndContentAs.GetContentAs<UserRegistrationResult>();
        return registerUserResponse!.UserId;
    }

    /// <summary>
    /// Registers a new user, confirms their registration using a verification code, and persists the user in the system.
    /// </summary>
    /// <param name="username">The username of the user to be registered and confirmed.</param>
    /// <param name="email">The email address of the user to be registered and confirmed.</param>
    /// <param name="password">The password for the user account being registered and confirmed.</param>
    /// <returns>The unique identifier of the newly registered and confirmed user.</returns>
    protected async Task<string> RegisterAndConfirmUser(string username, string email, string password)
    {
        string userID = await RegisterUser(username, email, password);
        VerificationCodeEntity codeResult = await WaitForVerificationCode(email, TimeSpan.FromSeconds(10));
        VerifyUserCommand verificationCommand = new VerifyUserCommand(email, codeResult.Code);
        HttpResponseMessage patchAsJsonAsync = await Client.PatchAsJsonAsync("api/v1/auth/confirm", verificationCommand);
        patchAsJsonAsync.EnsureSuccessStatusCode();
        return userID;
    }

    /// <summary>
    /// Authenticates a user by validating their credentials and retrieving authentication details upon success.
    /// </summary>
    /// <param name="email">The email address of the user attempting to authenticate.</param>
    /// <param name="password">The password associated with the user's account.</param>
    /// <returns>An instance of <c>AuthenticationResult</c> containing the user ID, access token, and refresh token.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the authentication result is null, indicating a failure to authenticate the user.</exception>
    protected async Task<AuthenticationResult> AuthenticateUser(string email, string password)
    {
        AuthenticateUserCommand command = new(email, password);
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/v1/auth/authenticate", command);
        ResponseEntity token = await response.GetAsResponseEntityAndContentAs<AuthenticationResult>();
        AuthenticationResult? result = token.GetContentAs<AuthenticationResult>();
        Assert.That(result, Is.Not.Null);
        return result ?? throw new InvalidOperationException("Authentication result was null.");
    }

    /// <summary>
    /// Creates a new admin user and persists it in the repository.
    /// </summary>
    /// <param name="username">The full name of the admin user to be created.</param>
    /// <param name="email">The email address of the admin user.</param>
    /// <param name="ID">The ID of the user to promote to admin</param>
    /// <returns>The unique identifier of the newly created admin user.</returns>
    protected async Task<string> PromoteToAdmin(string username, string email, string ID)
    {
        using IServiceScope scope = Factory.Services.CreateScope();
        IUserRepository repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        string userId = Guid.NewGuid().ToString();
        User adminUser = new()
        {
            Id = ID,
            CognitoId = userId,
            Email = email,
            FullName = username,
            Role = UserRole.Admin
        };

        await repository.AddAsync(adminUser);
            
        return userId;
    }

    /// <summary>
    /// Retrieves a user entity by their unique identifier from the user repository.
    /// </summary>
    /// <param name="userID">The unique identifier of the user to be retrieved.</param>
    /// <returns>A <see cref="User"/> object representing the user associated with the specified identifier, or null if the user is not found.</returns>
    protected async Task<User> CheckForUser(string userID)
    {
        using IServiceScope scope = Factory.Services.CreateScope();
        IUserRepository repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        Result<User> user = await repository.GetByIdAsync(userID);
        return user.GetValueOrDefault();
    }
}
