using System.Reflection;
using Amazon.Extensions.NETCore.Setup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Opc.AwsSettings.SystemsManager.AppConfig.FeatureFlags;

public static class FeatureFlagsExtensions
{
    public static IConfigurationBuilder AddFeatureFlags(this IConfigurationBuilder configurationBuilder,
        string environment, string configurationProfileIdentifier,
        TimeSpan? reloadAfter = null, ILogger? logger = null)
    {
        var applicationIdentifier = Assembly.GetCallingAssembly().GetName().Name ?? throw new NullReferenceException("Unable to parse ApplicationIdentifier. Calling Assembly Name is null");
        
        return AddFeatureFlags(configurationBuilder, applicationIdentifier, environment,
            configurationProfileIdentifier, reloadAfter, logger);
    }
    
    public static IConfigurationBuilder AddFeatureFlags(this IConfigurationBuilder configurationBuilder,
        string applicationIdentifier, string environment, string configurationProfileIdentifier,
        TimeSpan? reloadAfter = null, ILogger? logger = null)
    {
        return configurationBuilder.AddFeatureFlags(configurationBuilder.Build().GetAWSOptions(), applicationIdentifier,
            environment, configurationProfileIdentifier, reloadAfter, logger);
    }

    public static IConfigurationBuilder AddFeatureFlags(this IConfigurationBuilder configurationBuilder,
        AWSOptions awsOptions,
        string applicationIdentifier, string environment, string configurationProfileIdentifier,
        TimeSpan? reloadAfter = null, ILogger? logger = null)
    {
        var options = new FeatureFlagsConfigurationProviderOptions
        {
            AwsOptions = awsOptions,
            ApplicationIdentifier = applicationIdentifier,
            EnvironmentIdentifier = environment,
            ConfigurationProfileIdentifier = configurationProfileIdentifier,
            PollingInterval = reloadAfter
        };

        return configurationBuilder.AddFeatureFlags(options, logger);
    }

    public static IConfigurationBuilder AddFeatureFlags(this IConfigurationBuilder configurationBuilder,
        Action<FeatureFlagsConfigurationProviderOptions> configurator, ILogger? logger = null)
    {
        var options = new FeatureFlagsConfigurationProviderOptions()
        {
            AwsOptions = configurationBuilder.Build().GetAWSOptions()
        };
        
        configurator(options);
            
        return configurationBuilder.AddFeatureFlags(options, logger);
    }

    public static IConfigurationBuilder AddFeatureFlags(this IConfigurationBuilder configurationBuilder,
        FeatureFlagsConfigurationProviderOptions options, ILogger? logger = null)
    {
        logger?.AddedAppConfigFeatureFlags(options.ApplicationIdentifier, options.EnvironmentIdentifier, options.ConfigurationProfileIdentifier, options.PollingInterval);

        var source = new FeatureFlagsConfigurationSource(options, logger);

        configurationBuilder.Add(source);

        return configurationBuilder;
    }
}