using VibraHeka.Infrastructure;
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
