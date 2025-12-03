using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Common.Behaviours;
using Microsoft.Extensions.Hosting;
using VibraHeka.Application.Users.Commands;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddAutoMapper(cfg => 
            cfg.AddMaps(Assembly.GetExecutingAssembly()));

        builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        

        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
            cfg.LicenseKey =
                "eyJhbGciOiJSUzI1NiIsImtpZCI6Ikx1Y2t5UGVubnlTb2Z0d2FyZUxpY2Vuc2VLZXkvYmJiMTNhY2I1OTkwNGQ4OWI0Y2IxYzg1ZjA4OGNjZjkiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJodHRwczovL2x1Y2t5cGVubnlzb2Z0d2FyZS5jb20iLCJhdWQiOiJMdWNreVBlbm55U29mdHdhcmUiLCJleHAiOiIxNzk2MTY5NjAwIiwiaWF0IjoiMTc2NDcxNjYwMiIsImFjY291bnRfaWQiOiIwMTlhZTE0ZGZjZTY3OGY1OTQ1Y2UyYTk5NWIzYTQxYSIsImN1c3RvbWVyX2lkIjoiY3RtXzAxa2JnbXdydnJhMXMzbjFyZnI3em5tNGY2Iiwic3ViX2lkIjoiLSIsImVkaXRpb24iOiIwIiwidHlwZSI6IjIifQ.JK9w_CcUISE2heXbzk2SPQpPjxMQu7EvsoqRIUp312k5jurNm8h1iFlQSiOkq7k3f2A1DKkWrISL72perZPcQa7lnBVO2T7EwVlAJnc7LHLQ5lpE5u7RiTunyblRGirZY6X0EKZ_hbW77dmtxP4kyKgUdKyXtVAvZYWjimJKF130RxxUhqYp5lG6maFNJd0IxqA92AJ1Yf7eVzpJiO2DeY1338jGvfiuBrXZhWkszR2PSPukttnNQ0Z1l9YT03l94XuXcyZRHQfnU4RpDrMYlfpVXKpSWLenXW0r3SPR0ag3ptJjAmTsbz2BbqFlL5KyNVgGBqFU786ivhZRt1BqyA";
        });
        
        builder.Services.AddValidatorsFromAssemblyContaining<RegisterUserCommand>();
    }
}
