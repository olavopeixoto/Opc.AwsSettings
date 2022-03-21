using Microsoft.Extensions.Logging;

namespace Opc.AwsSettings.SystemsManager.AppConfig.FeatureFlags;
    
public static partial class LoggerMessageDefinitions
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "Error polling for changes in AppConfig")]
    public static partial void ErrorPollingForChanges(this ILogger logger, Exception exception);
    
    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "{Count} Feature Flags have been loaded")]
    public static partial void FeatureFlagsLoaded(this ILogger logger, int count);
    
    [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "Feature Flags values:\n{values}")]
    public static partial void FeatureFlagsValueLoaded(this ILogger logger, IDictionary<string, string>? values);

    [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "FeatureFlagsConfigurationProvider is disposed")]
    public static partial void FeatureFlagsConfigurationProviderDisposed(this ILogger logger);
    
    [LoggerMessage(EventId = 5, Level = LogLevel.Information, Message = "Added AWS AppConfig Feature Flags with Application Identifier: {ApplicationIdentifier}, Environment Name: {EnvironmentName}, Configuration Profile Identifier: {ConfigurationProfileIdentifier} and reloading after {ReloadAfter}")]
    public static partial void AddedAppConfigFeatureFlags(this ILogger logger, string applicationIdentifier, string environmentName, string configurationProfileIdentifier, TimeSpan? reloadAfter);
}
