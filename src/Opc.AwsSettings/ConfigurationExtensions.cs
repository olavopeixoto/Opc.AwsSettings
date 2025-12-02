using System.Text.RegularExpressions;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Opc.AwsSettings;
using Opc.AwsSettings.SecretsManager;
using Opc.AwsSettings.Settings;
using Opc.AwsSettings.SystemsManager.AppConfig.AppConfigData;
using Opc.AwsSettings.SystemsManager.ParameterStore;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Configuration;

public static class ConfigurationExtensions
{
    private static string GetEnvironmentName(string? environmentName)
    {
        return !string.IsNullOrWhiteSpace(environmentName)
            ? environmentName!
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
        ILogger? logger = null)
    {
        return AddAwsSettings(configurationBuilder, null, logger);
    }

    public static IConfigurationBuilder AddAwsSettings(this IConfigurationBuilder configurationBuilder,
        string? environmentName = null, ILogger? logger = null)
    {
        var currentConfiguration = configurationBuilder.Build();
        var awsSettings = currentConfiguration.GetSettings<AwsSettings>();

        if (awsSettings is null) return configurationBuilder;

        environmentName = GetEnvironmentName(environmentName);

        configurationBuilder.AddAppConfig(awsSettings.AppConfig, awsSettings.ReloadAfter, environmentName, logger);

        // Allow loading secrets from AppConfig
        currentConfiguration = configurationBuilder.Build();
        awsSettings = currentConfiguration.GetSettings<AwsSettings>();

        if (awsSettings is null) return configurationBuilder;

        configurationBuilder.AddParameterStore(awsSettings.ParameterStore, awsSettings.ReloadAfter, logger);

        configurationBuilder.AddSecretsManager(awsSettings.SecretsManager, awsSettings.ReloadAfter, environmentName,
            logger);

        return configurationBuilder;
    }

    public static IConfigurationBuilder AddParameterStore(this IConfigurationBuilder configurationBuilder,
        ParameterStoreSettings? settings, TimeSpan? reloadAfter, ILogger? logger = null)
    {
        if (settings is null) return configurationBuilder;

        var keys = settings.Keys
            .Union(settings.Paths
                .Where(path => settings.Keys.All(k => k.Path != path))
                .Select(path =>
                    new ParameterStoreKeySettings
                    {
                        Path = path
                    }));

        foreach (var key in keys)
            AddParameterStore(configurationBuilder, key, reloadAfter, logger);

        return configurationBuilder;
    }

    public static IConfigurationBuilder AddParameterStore(this IConfigurationBuilder configurationBuilder, ParameterStoreKeySettings key,
        TimeSpan? reloadAfter, ILogger? logger)
    {
        configurationBuilder.AddSystemsManager(options =>
        {
            options.Optional = false;
            options.Path = key.Path;
            options.ReloadAfter = reloadAfter;

            options.OnLoadException = exceptionContext =>
            {
                logger?.ErrorLoadingFromProvider(exceptionContext.Exception, exceptionContext.Provider.GetType().Name);
                exceptionContext.Ignore = key.Optional;
            };

            options.ParameterProcessor = new AwsSettingsParameterProcessor(logger, key);

            logger?.AddedParameterStore(key.Path, reloadAfter);
        });

        return configurationBuilder;
    }

    public static IConfigurationBuilder AddAppConfig(this IConfigurationBuilder configurationBuilder, AppConfigSettings? settings,
        TimeSpan? reloadAfter, string environmentName, ILogger? logger = null)
    {
        if (settings is null) return configurationBuilder;

        foreach (var config in settings.ConfigurationProfiles)
        {
            var source = new AppConfigDataSource(logger, settings.UseLambdaCacheLayer)
            {
                ApplicationIdentifier = settings.ApplicationIdentifier,
                ConfigurationProfileIdentifier = config.Identifier,
                EnvironmentIdentifier = environmentName,
                ReloadAfter = reloadAfter,
                Optional = false,
                OnLoadException = exceptionContext =>
                {
                    logger?.ErrorLoadingFromProvider(exceptionContext.Exception,
                        exceptionContext.Provider.GetType().Name);
                    exceptionContext.Ignore = config.Optional;
                }
            };

            configurationBuilder.AddAppConfig(source);

            logger?.AddedAppConfigDataConfiguration(settings.ApplicationIdentifier, environmentName, config.Identifier,
                reloadAfter);
        }

        return configurationBuilder;
    }

    public static IConfigurationBuilder AddSecretsManager(this IConfigurationBuilder configurationBuilder,
        SecretsManagerSettings? settings, TimeSpan? reloadAfter, string environmentName, ILogger? logger = null)
    {
        if (settings is null) return configurationBuilder;

        var secretsManagerPrefix = settings.Prefix?.TrimStart('/').TrimEnd('/');

        if (secretsManagerPrefix is null &&
            (settings.AcceptedSecretArns is null || settings.AcceptedSecretArns.Count <= 0))
            return configurationBuilder;

        configurationBuilder.AddSecretsManager(options =>
        {
            if (settings.AcceptedSecretArns is not null && settings.AcceptedSecretArns.Count > 0)
            {
                options.AcceptedSecretArns = settings.AcceptedSecretArns;

                logger?.AddedSecretsManager(options.AcceptedSecretArns, reloadAfter);
            }
            else
            {
                List<string> prefixes =
                [
                    $"{secretsManagerPrefix}/",
                    $"{environmentName}/"
                ];

                logger?.AddedSecretsManager(prefixes, reloadAfter);

                options.PollingInterval = reloadAfter;

                if (!settings.LoadAll)
                {
                    options.ListSecretsFilters = () =>
                    [
                        new Filter
                        {
                            Key = FilterNameStringType.Name,
                            Values = prefixes
                        }
                    ];

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

                    return result.Replace("/", ":");
                };
            }
        }, logger);

        return configurationBuilder;
    }
}
