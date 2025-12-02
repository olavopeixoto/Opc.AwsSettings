using Microsoft.Extensions.Logging;

namespace Opc.AwsSettings.SystemsManager.ParameterStore;

internal static partial class LoggerMessageDefinitions
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "Error parsing parameter {parameterName} with value {parameterValue}")]
    public static partial void ErrorLoadingParameter(this ILogger logger, Exception exception, string parameterName, string parameterValue);
}
