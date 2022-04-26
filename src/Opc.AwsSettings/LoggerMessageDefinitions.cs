using Microsoft.Extensions.Logging;

namespace Opc.AwsSettings;
    
public static partial class LoggerMessageDefinitions
{
    public static void AddedParameterStore(this ILogger logger, string path, TimeSpan? reloadAfter)
    {
        if (reloadAfter.HasValue)
        {
            logger.AddedParameterStoreWithReload(path, reloadAfter.Value);
        }
        else
        {
            logger.AddedParameterStoreWithoutReload(path);
        }
    }

    public static void AddedSecretsManagerKey(this ILogger logger, string path, TimeSpan? reloadAfter)
    {
        if (reloadAfter.HasValue)
        {
            logger.AddedSecretsManagerKeyWithReload(path, reloadAfter.Value);
        }
        else
        {
            logger.AddedSecretsManagerKeyWithoutReload(path);
        }
    }

    public static void AddedSecretsManager(this ILogger logger, IEnumerable<string> paths, TimeSpan? reloadAfter)
    {
        if (reloadAfter.HasValue)
        {
            logger.AddedSecretsManagerWithReload(paths, reloadAfter.Value);
        }
        else
        {
            logger.AddedSecretsManagerWithoutReload(paths);
        }
    }

    public static void AddedAppConfigFreeformConfiguration(this ILogger logger, string applicationIdentifier, string environmentName, string configurationProfileIdentifier, TimeSpan? reloadAfter)
    {
        if (reloadAfter.HasValue)
        {
            logger.AddedAppConfigFreeformConfigurationWithReload(applicationIdentifier, environmentName, configurationProfileIdentifier, reloadAfter.Value);
        }
        else
        {
            logger.AddedAppConfigFreeformConfigurationWithoutReload(applicationIdentifier, environmentName, configurationProfileIdentifier);
        }
    }
    
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Added AWS Parameter Store using path: {Path} and reloading after: {ReloadAfter}")]
    public static partial void AddedParameterStoreWithReload(this ILogger logger, string path, TimeSpan reloadAfter);
    
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Added AWS Parameter Store using path: {Path} and not reloading")]
    public static partial void AddedParameterStoreWithoutReload(this ILogger logger, string path);
    
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Added AWS Secrets Manager key: {Path} and reloading after: {ReloadAfter}")]
    public static partial void AddedSecretsManagerKeyWithReload(this ILogger logger, string path, TimeSpan reloadAfter);
    
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Added AWS Secrets Manager key: {Path} and not reloading")]
    public static partial void AddedSecretsManagerKeyWithoutReload(this ILogger logger, string path);
    
    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Added AWS Secrets Manager using paths: {Paths} and reloading after: {ReloadAfter}")]
    public static partial void AddedSecretsManagerWithReload(this ILogger logger, IEnumerable<string> paths, TimeSpan reloadAfter);
    
    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Added AWS Secrets Manager using paths: {Paths} and not reloading")]
    public static partial void AddedSecretsManagerWithoutReload(this ILogger logger, IEnumerable<string> paths);
    
    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Added AWS AppConfig Freeform Configuration with Application Identifier: {ApplicationIdentifier}, Environment Name: {EnvironmentName}, Configuration Profile Identifier: {ConfigurationProfileIdentifier} and reloading after {ReloadAfter}")]
    public static partial void AddedAppConfigFreeformConfigurationWithReload(this ILogger logger, string applicationIdentifier, string environmentName, string configurationProfileIdentifier, TimeSpan reloadAfter);
    
    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Added AWS AppConfig Freeform Configuration with Application Identifier: {ApplicationIdentifier}, Environment Name: {EnvironmentName}, Configuration Profile Identifier: {ConfigurationProfileIdentifier} and not reloading.")]
    public static partial void AddedAppConfigFreeformConfigurationWithoutReload(this ILogger logger, string applicationIdentifier, string environmentName, string configurationProfileIdentifier);
    
    [LoggerMessage(EventId = 4, Level = LogLevel.Warning, Message = "Error loading from {providerType}")]
    public static partial void ErrorLoadingFromParameterStore(this ILogger logger, Exception exception, string providerType);
}
