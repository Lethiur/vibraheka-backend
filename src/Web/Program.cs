using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Infrastructure.AWS;
using Infrastructure.Stripe;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using VibraHeka.Application;
using VibraHeka.Domain;
using VibraHeka.Infrastructure;
using VibraHeka.Infrastructure.Middlewares;
using VibraHeka.Web.Middleware;
using static System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler;

namespace VibraHeka.Web;

public class VibraHekaProgram
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
        
        
        builder.AddInfrastructureServices(builder.Configuration, builder.Configuration);
        builder.AddDomainServices(builder.Configuration, builder.Configuration);
        builder.AddPaymentServices(builder.Configuration, builder.Configuration);
        builder.AddCloudServices(builder.Configuration, builder.Configuration);
        
        IConfigurationSection settingsSection = builder.Configuration.GetSection("Settings");
        bool useSerilog = settingsSection.GetValue<bool>("UseSerilog");
        if (useSerilog)
        {
            builder.ConfigureLogging(builder.Configuration, builder.Configuration);

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
