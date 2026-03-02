using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Amazon;
using Amazon.CloudWatchLogs;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Sinks.AwsCloudWatch;
using VibraHeka.Application;
using VibraHeka.Infrastructure;
using VibraHeka.Web.Middleware;
using static System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler;

namespace VibraHeka.Web;

public partial class VibraHekaProgram
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        DefaultInboundClaimTypeMap.Clear();
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend",
                policy =>
                {
                    policy.WithOrigins("http://localhost:5173") // La URL de tu frontend
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials(); 
                });
        });

        builder.AddApplicationServices();
        builder.AddWebServices();
        builder.Services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        });;
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                
                string? region = builder.Configuration["AWS:Location"];
                string? userPoolId = builder.Configuration["AWS:UserPoolId"];
                string? clientId = builder.Configuration["AWS:ClientId"];

                options.Authority = $"https://cognito-idp.{region}.amazonaws.com/{userPoolId}";
                options.RequireHttpsMetadata = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = options.Authority,
                    ValidateAudience = true,
                    ValidAudience = clientId,
                    
                    ClockSkew = TimeSpan.FromMinutes(2),
                    ValidateLifetime = true,
                    AudienceValidator = (audiences, securityToken, validationParameters) =>
                    {
                        switch (securityToken)
                        {
                            case JsonWebToken jwt:
                                {
                                    string? clientIdClaim = jwt.GetClaim("client_id")?.Value;
                                    return clientIdClaim == clientId;
                                }
                            case JwtSecurityToken oldJwt:
                                return oldJwt.Payload.TryGetValue("client_id", out object? cid) && cid.ToString() == clientId;
                            default:
                                return false;
                        }
                    }
                };
            });
        IConfigurationSection settingsSection = builder.Configuration.GetSection("Settings");
        builder.AddInfrastructureServices(builder.Configuration, builder.Configuration);
        
        bool useSerilog = settingsSection.GetValue<bool>("UseSerilog");
        if (useSerilog)
        {
            builder.Host.UseSerilog((context, services, configuration) => 
            {
                configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext();

                string? profile = context.Configuration["AWS:Profile"] ?? context.Configuration["AWSLogging:Profile"];
                string regionName = context.Configuration["AWS:Location"]
                    ?? context.Configuration["AWSLogging:Region"]
                    ?? RegionEndpoint.EUWest1.SystemName;

                RegionEndpoint region = RegionEndpoint.GetBySystemName(regionName);
                AWSCredentials? credentials = null;

                if (!string.IsNullOrWhiteSpace(profile))
                {
                    CredentialProfileStoreChain profileChain = new();
                    profileChain.TryGetAWSCredentials(profile, out credentials);
                }

                IAmazonCloudWatchLogs cloudWatchClient = credentials is null
                    ? new AmazonCloudWatchLogsClient(region)
                    : new AmazonCloudWatchLogsClient(credentials, region);

                configuration.WriteTo.AmazonCloudWatch(
                    new CloudWatchSinkOptions
                    {
                        LogGroupName = context.Configuration["AWSLogging:LogGroup"] ?? "/my-app/logs",
                        BatchSizeLimit = context.Configuration.GetValue<int?>("Serilog:WriteTo:1:Args:batchSizeLimit") ?? 100,
                        CreateLogGroup = context.Configuration.GetValue<bool?>("Serilog:WriteTo:1:Args:createLogGroup") ?? true,
                        Period = TimeSpan.FromSeconds(1),
                        TextFormatter = new RenderedCompactJsonFormatter()
                    },
                    cloudWatchClient);
            }, preserveStaticLogger: true);    
        }
        
        WebApplication app = builder.Build();
        if (useSerilog)
        {
            app.UseSerilogRequestLogging();    
        }
        
        app.UseRouting();
        app.UseCors("AllowFrontend");
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.UseXRay("VibraHeka", builder.Configuration);
        
        app.UseMiddleware<TracingMiddleware>();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseStaticFiles();

        app.MapControllers();

        app.UseSwaggerUi(settings =>
        {
            settings.Path = "/api";
            settings.DocumentPath = "/api/specification.json";
        });


        app.Run();
    }
}
