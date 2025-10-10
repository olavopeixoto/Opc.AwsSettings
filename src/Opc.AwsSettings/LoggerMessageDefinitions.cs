using Microsoft.Extensions.Logging;

namespace Opc.AwsSettings;

internal static partial class LoggerMessageDefinitions
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information,
        Message = "Added AWS Parameter Store using path: {Path} and reloading after: {ReloadAfter}")]
    public static partial void AddedParameterStore(this ILogger logger, string path, TimeSpan? reloadAfter);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information,
        Message = "Added AWS Secrets Manager key: {Path} and reloading after: {ReloadAfter}")]
    public static partial void AddedSecretsManagerKey(this ILogger logger, string path, TimeSpan? reloadAfter);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information,
        Message = "Added AWS Secrets Manager using paths: {Paths} and reloading after: {ReloadAfter}")]
    public static partial void AddedSecretsManager(this ILogger logger, IEnumerable<string> paths,
        TimeSpan? reloadAfter);

    [LoggerMessage(EventId = 4, Level = LogLevel.Information,
        Message =
            "Added AWS AppConfig Configuration with Application Identifier: {ApplicationIdentifier}, Environment Name: {EnvironmentName}, Configuration Profile Identifier: {ConfigurationProfileIdentifier} and reloading after {ReloadAfter}")]
    public static partial void AddedAppConfigDataConfiguration(this ILogger logger, string applicationIdentifier,
        string? environmentName, string configurationProfileIdentifier, TimeSpan? reloadAfter);

    [LoggerMessage(EventId = 5, Level = LogLevel.Warning, Message = "Error loading from {providerType}")]
    public static partial void ErrorLoadingFromProvider(this ILogger logger, Exception exception, string providerType);
}
