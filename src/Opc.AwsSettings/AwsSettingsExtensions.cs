using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Configuration;

public static class AwsSettingsExtensions
{
    /// <summary>
    /// Add configuration source from AWS Parameter Store, Secrets Manager, AppConfig Freeform Configuration and AppConfig Feature Flags
    /// </summary>
    /// <param name="builder">HostBuilder</param>
    /// <param name="logger">Optional logger to return information about data being loaded</param>
    /// <returns></returns>
    public static IHostBuilder AddAwsSettings(this IHostBuilder builder, ILogger? logger = null)
    {
        builder.ConfigureAppConfiguration((context, configurationBuilder) =>
        {
            configurationBuilder.AddAwsSettings(context.HostingEnvironment.EnvironmentName, logger);
        });

        return builder;
    }
}