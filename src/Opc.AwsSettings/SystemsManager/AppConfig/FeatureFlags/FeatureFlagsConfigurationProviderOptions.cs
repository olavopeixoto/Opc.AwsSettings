using Amazon.Extensions.NETCore.Setup;

namespace Opc.AwsSettings.SystemsManager.AppConfig.FeatureFlags;

public record FeatureFlagsConfigurationProviderOptions
{
    /// <summary>
    /// The time that should be waited before refreshing the Feature Flags.
    /// If null, Feature Flags will not be refreshed.
    /// </summary>
    /// <example>
    /// <code>
    /// PollingInterval = TimeSpan.FromMinutes(15);
    /// </code>
    /// </example>
    public TimeSpan? PollingInterval { get; init; }
    
    public string ApplicationIdentifier { get; init; }

    public string ConfigurationProfileIdentifier { get; init; }
    
    internal string EnvironmentIdentifier { get; init; }

    /// <summary>
    /// AwsOptions
    /// </summary>
    public AWSOptions AwsOptions { get; init; } = new();
}