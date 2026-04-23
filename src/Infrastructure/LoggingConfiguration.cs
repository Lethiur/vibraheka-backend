using Amazon;
using Amazon.CloudWatchLogs;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Sinks.AwsCloudWatch;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Loggers;

namespace VibraHeka.Infrastructure;

public static class LoggingConfiguration
{
    public static void ConfigureLogging(this WebApplicationBuilder builder, IConfiguration config,
        ConfigurationManager configurationManager)
    {
        builder.Host.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.With(new XRayEnricher());

            AWSConfig? awsConfig = config.GetSection("AWS").Get<AWSConfig>();
            AWSLoggingConfig? loggingConfig = config.GetSection("AWSLogging").Get<AWSLoggingConfig>();
            RegionEndpoint regionEndpoint = RegionEndpoint.GetBySystemName(awsConfig!.Location);

            new CredentialProfileStoreChain().TryGetAWSCredentials(awsConfig.Profile, out AWSCredentials credentials);

            if (credentials == null)
            {
                throw new InvalidOperationException(
                    $"Failed to retrieve AWS credentials for profile '{awsConfig.Profile}'");
            }

            IAmazonCloudWatchLogs cloudWatchClient = new AmazonCloudWatchLogsClient(credentials, regionEndpoint);

            configuration.WriteTo.AmazonCloudWatch(
                new CloudWatchSinkOptions()
                {
                    LogGroupName = loggingConfig!.LogGroup,
                    BatchSizeLimit =
                        context.Configuration.GetValue<int?>("Serilog:WriteTo:1:Args:batchSizeLimit") ?? 100,
                    CreateLogGroup =
                        context.Configuration.GetValue<bool?>("Serilog:WriteTo:1:Args:createLogGroup") ?? true,
                    Period = TimeSpan.FromSeconds(1),
                    TextFormatter = new RenderedCompactJsonFormatter()
                }, cloudWatchClient);
        }, preserveStaticLogger: true);
    }
}
