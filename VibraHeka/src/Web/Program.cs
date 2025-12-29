using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using VibraHeka.Application;
using VibraHeka.Infrastructure;
using VibraHeka.Web;
using VibraHeka.Web.Middleware;

public class VibraHekaProgram
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
        builder.AddApplicationServices();
        builder.AddWebServices();
        builder.Services.AddControllers();
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
            });
        builder.AddInfrastructureServices(builder.Configuration);
        var app = builder.Build();


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
