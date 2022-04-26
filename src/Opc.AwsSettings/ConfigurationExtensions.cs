using System.Text.RegularExpressions;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Opc.AwsSettings;
using Opc.AwsSettings.SecretsManager;
using Opc.AwsSettings.Settings;
using Opc.AwsSettings.SystemsManager.AppConfig.FeatureFlags;
using Opc.AwsSettings.SystemsManager.ParameterStore;

public static class ConfigurationExtensions
{
    /// <summary>
    ///     Attempts to bind the configuration instance to a new instance of type T using the configuration section with the
    ///     type name.
    /// </summary>
    /// <param name="configuration">The configuration instance to bind.</param>
    /// <typeparam name="T">The type of the new instance to bind.</typeparam>
    /// <returns>The new instance of T if successful, default(T) otherwise.</returns>
    public static T GetSettings<T>(this IConfiguration configuration)
    {

        return configuration.GetSection(typeof(T).Name).Get<T>();
    }

    public static IConfigurationBuilder AddAwsSettings(this IConfigurationBuilder configurationBuilder, string? environmentName = null, ILogger? logger = null)
    {
        environmentName = string.IsNullOrWhiteSpace(environmentName) ? "Production" : environmentName;

        var currentConfiguration = configurationBuilder.Build();
        var awsSettings = currentConfiguration.GetSettings<AwsSettings>();

        if (awsSettings?.ParameterStore?.Keys is null) return configurationBuilder;

        var keys = awsSettings.ParameterStore.Keys
            .Union(awsSettings.ParameterStore.Paths
                .Where(path => awsSettings.ParameterStore.Keys.All(k => k.Path != path))
                .Select(path =>
                    new ParameterStoreKeySettings
                    {
                        Path = path,
                    }));

        foreach (var key in keys)
        {
            configurationBuilder.AddParameterStore(key.Path, key.Alias, awsSettings.ReloadAfter, logger);
        }

        var secretsManagerPrefix = awsSettings.SecretsManager.Prefix?.TrimStart('/').TrimEnd('/');
        if (secretsManagerPrefix is not null || awsSettings.SecretsManager.AcceptedSecretArns is not null && awsSettings.SecretsManager.AcceptedSecretArns.Count > 0)
        {
            configurationBuilder.AddSecretsManager(configurator: options =>
            {
                if (awsSettings.SecretsManager.AcceptedSecretArns is not null && awsSettings.SecretsManager.AcceptedSecretArns.Count > 0)
                {
                    options.AcceptedSecretArns = awsSettings.SecretsManager.AcceptedSecretArns;

                    logger?.AddedSecretsManager(options.AcceptedSecretArns, awsSettings.ReloadAfter);
                }
                else
                {
                    var prefixes = new[]
                    {
                        $"{secretsManagerPrefix}/",
                        $"{environmentName.ToLowerInvariant()}/"
                    };

                    logger?.AddedSecretsManager(prefixes, awsSettings.ReloadAfter);

                    options.PollingInterval = awsSettings.ReloadAfter;

                    var regexes = prefixes.Select(prefix => new Regex(Regex.Escape(prefix),
                        RegexOptions.IgnoreCase & RegexOptions.Singleline & RegexOptions.Compiled));

                    if (!awsSettings.SecretsManager.LoadAll)
                    {
                        options.ListSecretsFilters = new List<Filter>
                            {
                                new()
                                {
                                    Key = FilterNameStringType.Name,
                                    Values = prefixes.ToList()
                                }
                            };

                        options.SecretFilter = secret => prefixes.Any(prefix => secret.Name.StartsWith(prefix)) || options.AcceptedSecretArns.Any();
                    }

                    options.KeyGenerator = (_, key) =>
                    {
                        var result = regexes.Aggregate(key, (current, regex) => regex.Replace(current, string.Empty, 1));

                        return result.Replace(@"/", ":");
                    };
                }

            }, logger);
        }

        foreach (var featureSettings in awsSettings.AppConfig.FreeformConfigurations)
        {
            logger?.AddedAppConfigFreeformConfiguration(awsSettings.AppConfig.ApplicationIdentifier, environmentName, featureSettings.ConfigurationProfileIdentifier, awsSettings.ReloadAfter);

            configurationBuilder.AddAppConfig(
                awsSettings.AppConfig.ApplicationIdentifier,
                environmentName,
                featureSettings.ConfigurationProfileIdentifier,
                awsSettings.ReloadAfter);
        }

        foreach (var featureSettings in awsSettings.AppConfig.FeatureFlags)
        {
            configurationBuilder.AddFeatureFlags(
                awsSettings.AppConfig.ApplicationIdentifier,
                environmentName,
                featureSettings.ConfigurationProfileIdentifier,
                awsSettings.ReloadAfter,
                logger);
        }

        return configurationBuilder;
    }

    private static void AddParameterStore(this IConfigurationBuilder configurationBuilder, string path, string? alias, TimeSpan? reloadAfter, ILogger? logger)
    {
        configurationBuilder.AddSystemsManager(options =>
        {
            options.Path = path;
            options.Prefix = alias;
            options.ReloadAfter = reloadAfter;
            
            options.OnLoadException = exceptionContext =>
            {
                logger?.ErrorLoadingFromParameterStore(exceptionContext.Exception, exceptionContext.Provider.GetType().Name);
                exceptionContext.Ignore = options.Optional;
            };

            options.ParameterProcessor = new ArraySupportParameterProcessor();
    
            logger?.AddedParameterStore(path, reloadAfter);
        });
    }
}