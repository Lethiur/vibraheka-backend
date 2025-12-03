using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

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
}
