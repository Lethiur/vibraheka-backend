using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using VibraHeka.Application;
using VibraHeka.Infrastructure;
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
// Add services to the container.
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
                
                string? region = builder.Configuration["AWS:Region"];
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
        WebApplication app = builder.Build();
        app.UseCors("AllowFrontend");

// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseHttpsRedirection();
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
