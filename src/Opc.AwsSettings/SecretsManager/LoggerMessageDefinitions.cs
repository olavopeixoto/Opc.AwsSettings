using Microsoft.Extensions.Logging;

namespace Opc.AwsSettings.SecretsManager;

public static partial class LoggerMessageDefinitions
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "Error polling for changes in Secrets Manager")]
    public static partial void ErrorPollingForChanges(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information,
        Message = "{count} secrets have been loaded from Secrets Manager")]
    public static partial void SecretsLoaded(this ILogger logger, int count);

    [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "Loaded secret ARN {SecretArn}")]
    public static partial void SecretLoaded(this ILogger logger, string secretArn);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information,
        Message = "SecretsManagerConfigurationProvider is disposed")]
    public static partial void SecretsManagerConfigurationProviderDisposed(this ILogger logger);

    [LoggerMessage(EventId = 4, Level = LogLevel.Error,
        Message = "Error retrieving secret value (Secret: {SecretName} Arn: {SecretARN})")]
    public static partial void ErrorLoadingSecret(this ILogger logger, string secretName, string secretArn,
        Exception exception);
}