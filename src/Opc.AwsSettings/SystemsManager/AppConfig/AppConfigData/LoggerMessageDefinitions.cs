using Microsoft.Extensions.Logging;

namespace Opc.AwsSettings.SystemsManager.AppConfig.AppConfigData;

internal static partial class LoggerMessageDefinitions
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "{Count} Parameters have been loaded")]
    public static partial void ParametersLoaded(this ILogger logger, int count);

    [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "Parameters values:\n{values}")]
    public static partial void ParametersValueLoaded(this ILogger logger, IDictionary<string, string>? values);
}
