using System.Reflection;
using Amazon.Extensions.Configuration.SystemsManager.Internal;
using Microsoft.Extensions.Configuration;

namespace Opc.AwsSettings.SystemsManager.AppConfig.FeatureFlags;

public static class AppConfigExtensions
{
    public static IConfigurationBuilder AddFeatureFlags(this IConfigurationBuilder configurationBuilder, FeatureFlagsConfigurationSource source)
    {
        source.ApplicationIdentifier ??= Assembly.GetCallingAssembly().GetName().Name ?? throw new NullReferenceException("Unable to parse ApplicationIdentifier. Calling Assembly Name is null");
        
        if (string.IsNullOrWhiteSpace(source.ConfigurationProfileIdentifier))
            throw new ArgumentNullException(nameof(source.ConfigurationProfileIdentifier));

        source.AwsOptions ??= AwsOptionsProvider.GetAwsOptions(configurationBuilder);

        configurationBuilder.Add(source);

        return configurationBuilder;
    }
}