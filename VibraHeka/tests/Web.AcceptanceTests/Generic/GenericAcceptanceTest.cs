using System.Net.Http.Json;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Runtime;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using VibraHeka.Application.Common.Models.Results;
using VibraHeka.Application.Users.Commands;
using VibraHeka.Application.Users.Queries.GetCode;
using VibraHeka.Domain.Entities;
using Table = TechTalk.SpecFlow.Table;

namespace VibraHeka.Web.AcceptanceTests.Generic;

public class GenericAcceptanceTest<TAppClass> where TAppClass : class
{
    protected HttpClient Client = null!;
    private readonly WebApplicationFactory<TAppClass> Factory;

    public GenericAcceptanceTest()
    {
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
    
    protected async Task<string> RegisterAndConfirmUser(string username, string email, string password)
    {
        string userID = await RegisterUser(username, email, password);
        VerificationCodeEntity codeResult = await WaitForVerificationCode(email, TimeSpan.FromSeconds(10));
        await Client.PatchAsync($"api/v1/users/{username}/confirm/with/{codeResult.Code}", null);
        return userID;
    }
}
