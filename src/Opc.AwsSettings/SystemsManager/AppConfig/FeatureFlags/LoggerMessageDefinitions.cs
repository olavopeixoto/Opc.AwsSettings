using Microsoft.Extensions.Logging;

namespace Opc.AwsSettings.SystemsManager.AppConfig.FeatureFlags;
    
public static partial class LoggerMessageDefinitions
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "{Count} Feature Flags have been loaded")]
    public static partial void FeatureFlagsLoaded(this ILogger logger, int count);
    
    [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "Feature Flags values:\n{values}")]
    public static partial void FeatureFlagsValueLoaded(this ILogger logger, IDictionary<string, string>? values);
}
