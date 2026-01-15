using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using VibraHeka.Application;
using VibraHeka.Infrastructure;
using VibraHeka.Web;
using VibraHeka.Web.Middleware;
using static System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler;

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
                        .AllowCredentials(); // Importante si usas cookies o autenticación Windows
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
                string? region = builder.Configuration["Cognito:Region"];
                string? userPoolId = builder.Configuration["Cognito:UserPoolId"];
                string? clientId = builder.Configuration["Cognito:ClientId"];

                options.Authority = $"https://cognito-idp.{region}.amazonaws.com/{userPoolId}";
                options.RequireHttpsMetadata = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = options.Authority,
                    ValidateAudience = true,
                    ValidAudience = clientId,
                    
                    ClockSkew = TimeSpan.FromMinutes(2),
                    ValidateLifetime = true
                };
                
                options.TokenValidationParameters.AudienceValidator = (audiences, securityToken, validationParameters) =>
                {
                    // Usamos el token que viene de la petición
                    // En .NET moderno suele ser JsonWebToken
                    if (securityToken is Microsoft.IdentityModel.JsonWebTokens.JsonWebToken jwt)
                    {
                        string? clientIdClaim = jwt.GetClaim("client_id")?.Value;
                        return clientIdClaim == clientId;
                    }
                
                    if (securityToken is JwtSecurityToken oldJwt)
                    {
                        return oldJwt.Payload.TryGetValue("client_id", out object? cid) && cid.ToString() == clientId;
                    }

                    return false;
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
