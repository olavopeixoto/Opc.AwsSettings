using System.Text.RegularExpressions;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Opc.AwsSettings;
using Opc.AwsSettings.SecretsManager;
using Opc.AwsSettings.Settings;
using Opc.AwsSettings.SystemsManager.AppConfig.FeatureFlags;
using Opc.AwsSettings.SystemsManager.AppConfig.FreeForm;
using Opc.AwsSettings.SystemsManager.ParameterStore;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Configuration;

public static class ConfigurationExtensions
{
    private static string GetEnvironmentName(string? environmentName)
    {
        return !string.IsNullOrWhiteSpace(environmentName) ? environmentName
                                                             : Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                                                             ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                                                             ?? Environments.Production;
    }

    /// <summary>
    ///     Attempts to bind the configuration instance to a new instance of type T using the configuration section with the
    ///     type name.
    /// </summary>
    /// <param name="configuration">The configuration instance to bind.</param>
    /// <typeparam name="T">The type of the new instance to bind.</typeparam>
    /// <returns>The new instance of T if successful, default(T) otherwise.</returns>
    public static T? GetSettings<T>(this IConfiguration configuration)
    {
        return configuration
            .GetSection(typeof(T).Name)
            .Get<T>();
    }

    public static IConfigurationBuilder AddAwsSettings(this IConfigurationBuilder configurationBuilder,
        string? environmentName = null, ILogger? logger = null)
    {
        var currentConfiguration = configurationBuilder.Build();
        var awsSettings = currentConfiguration.GetSettings<AwsSettings>();

        if (awsSettings is null) return configurationBuilder;

        environmentName = GetEnvironmentName(environmentName);

        AddParameterStore(configurationBuilder, awsSettings.ParameterStore, awsSettings.ReloadAfter, logger);

        AddAppConfig(configurationBuilder, awsSettings.AppConfig, awsSettings.ReloadAfter, environmentName, logger);

        AddSecretsManager(configurationBuilder, awsSettings.SecretsManager, awsSettings.ReloadAfter, environmentName, logger);

        return configurationBuilder;
    }

    private static void AddParameterStore(this IConfigurationBuilder configurationBuilder, ParameterStoreSettings? settings, TimeSpan? reloadAfter, ILogger? logger = null)
    {
        if (settings is null) return;

        var keys = settings.Keys
            .Union(settings.Paths
                .Where(path => settings.Keys.All(k => k.Path != path))
                .Select(path =>
                    new ParameterStoreKeySettings
                    {
                        Path = path,
                    }));

        foreach (var key in keys)
        {
            configurationBuilder.AddParameterStore(key, reloadAfter, logger);
        }
    }

    private static void AddParameterStore(this IConfigurationBuilder configurationBuilder, ParameterStoreKeySettings key, TimeSpan? reloadAfter, ILogger? logger)
    {
        configurationBuilder.AddSystemsManager(options =>
        {
            options.Path = key.Path;
            options.Prefix = key.Alias;
            options.ReloadAfter = reloadAfter;

            options.OnLoadException = exceptionContext =>
            {
                logger?.ErrorLoadingFromParameterStore(exceptionContext.Exception, exceptionContext.Provider.GetType().Name);
                exceptionContext.Ignore = options.Optional;
            };

            options.ParameterProcessor = new ArraySupportParameterProcessor();

            logger?.AddedParameterStore(key.Path, reloadAfter);
        });
    }

    private static void AddAppConfig(this IConfigurationBuilder configurationBuilder, AppConfigSettings? settings,
        TimeSpan? reloadAfter, string environmentName, ILogger? logger = null)
    {
        if (settings is null) return;

        foreach (var freeFormConfig in settings.FreeFormConfigurations)
        {
            var source = new AppConfigFreeFormConfigurationSource(logger, settings.UseLambdaCacheLayer)
            {
                ApplicationId = settings.ApplicationIdentifier,
                ConfigProfileId = freeFormConfig.ConfigurationProfileIdentifier,
                EnvironmentId = environmentName,
                ReloadAfter = reloadAfter,
                Optional = false,
                ClientId = Guid.NewGuid().ToString(),
                OnLoadException = exceptionContext =>
                {
                    logger?.ErrorLoadingFromParameterStore(exceptionContext.Exception, exceptionContext.Provider.GetType().Name);
                    exceptionContext.Ignore = freeFormConfig.Optional;
                }
            };

            configurationBuilder.AddFreeFormAppConfig(source);

            logger?.AddedAppConfigFreeformConfiguration(settings.ApplicationIdentifier, environmentName, freeFormConfig.ConfigurationProfileIdentifier, reloadAfter);
        }

        foreach (var featureSettings in settings.FeatureFlags)
        {
            var source = new FeatureFlagsConfigurationSource(logger)
            {
                ApplicationIdentifier = settings.ApplicationIdentifier,
                ConfigurationProfileIdentifier = featureSettings.ConfigurationProfileIdentifier,
                EnvironmentIdentifier = environmentName,
                ReloadAfter = reloadAfter,
                Optional = false,
                OnLoadException = exceptionContext =>
                {
                    logger?.ErrorLoadingFromParameterStore(exceptionContext.Exception, exceptionContext.Provider.GetType().Name);
                    exceptionContext.Ignore = featureSettings.Optional;
                }
            };

            configurationBuilder.AddFeatureFlags(source);

            logger?.AddedAppConfigFeatureFlags(settings.ApplicationIdentifier, environmentName, featureSettings.ConfigurationProfileIdentifier, reloadAfter);
        }
    }

    private static void AddSecretsManager(this IConfigurationBuilder configurationBuilder,
        SecretsManagerSettings? settings, TimeSpan? reloadAfter, string environmentName, ILogger? logger = null)
    {
        if (settings is null) return;

        var secretsManagerPrefix = settings.Prefix?.TrimStart('/').TrimEnd('/');

        if (secretsManagerPrefix is null &&
            (settings.AcceptedSecretArns is null || settings.AcceptedSecretArns.Count <= 0))
            return;

        configurationBuilder.AddSecretsManager(configurator: options =>
        {
            if (settings.AcceptedSecretArns is not null && settings.AcceptedSecretArns.Count > 0)
            {
                options.AcceptedSecretArns = settings.AcceptedSecretArns;

                logger?.AddedSecretsManager(options.AcceptedSecretArns, reloadAfter);
            }
            else
            {
                IEnumerable<string> prefixes = new[]
                {
                    $"{secretsManagerPrefix}/", 
                    $"{environmentName}/"
                };

                logger?.AddedSecretsManager(prefixes, reloadAfter);

                options.PollingInterval = reloadAfter;

                if (!settings.LoadAll)
                {
                    options.ListSecretsFilters = () => new List<Filter>
                    {
                        new()
                        {
                            Key = FilterNameStringType.Name,
                            Values = prefixes.ToList()
                        }
                    };

                    options.SecretFilter = secret =>
                    {
                        return prefixes.Any(prefix => secret.Name.StartsWith(prefix))
                               || options.AcceptedSecretArns.Any();
                    };
                }

                var regexs = prefixes.Select(prefix => new Regex(Regex.Escape(prefix),
                    RegexOptions.IgnoreCase & RegexOptions.Singleline & RegexOptions.Compiled));

                options.KeyGenerator = (_, key) =>
                {
                    var result = regexs.Aggregate(key, (current, regex) => regex.Replace(current, string.Empty, 1));

                    return result.Replace(@"/", ":");
                };
            }

        }, logger);
    }
}